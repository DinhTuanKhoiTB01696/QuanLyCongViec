const fs = require('fs');
const path = require('path');

const srcPath = path.join('d:', 'A', 'QuanLyCongViec', 'Frontend', 'src');
const spaceFile = path.join(srcPath, 'views', 'SpaceSummary.vue');
const modalFile = path.join(srcPath, 'components', 'TaskDetailModal.vue');

let content = fs.readFileSync(spaceFile, 'utf8');

// Extract Modal HTML
const startStr = '<transition name="fade">\n        <div class="task-modal-overlay"';
let startIndex = content.indexOf(startStr);
// The HTML ends right before "      <!-- Nexus Layout handles Topbar and Sidebar -->"
const afterStr = '<!-- Nexus Layout handles Topbar and Sidebar -->';
let nextSection = content.indexOf(afterStr, startIndex);
// Reverse find </transition>
const endStr = '</transition>';
let endIndex = content.lastIndexOf(endStr, nextSection) + endStr.length;

const modalTemplate = content.substring(startIndex, endIndex);

// Build TaskDetailModal.vue
let modalVue = <template>\n   + modalTemplate.trim() + \n</template>\n\n<script setup>\n;
modalVue += import { ref, computed, watch, nextTick } from 'vue';\n;
modalVue += import { ElMessage, ElNotification } from 'element-plus';\n;
modalVue += import axiosClient from '@/api/axiosClient';\n\n;

modalVue += const props = defineProps({
  selectedTask: {
    type: Object,
    default: null
  },
  projectId: {
    type: [String, Number],
    required: true
  },
  projectMembers: {
    type: Array,
    default: () => []
  },
  currentUser: {
    type: Object,
    default: () => ({})
  }
});\n\n;

modalVue += const emit = defineEmits(['updateTask', 'close', 'open-task', 'create-subtask']);\n\n;

// Copying some utility methods commonly used in the template
modalVue += const formatDate = (dateStr) => {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN');
};\n\n;

modalVue += const updateTaskField = (task, field, value) => {
  emit('updateTask', task, field, value);
};\n\n;

modalVue += const openTaskDetail = (task) => {
  emit('open-task', task);
};\n\n;

modalVue += const createSubtask = (task) => {
  emit('create-subtask', task);
};\n\n;

modalVue += 
// ==========================================
// COMMENTS & ACTIVITY LOGIC
// ==========================================
const comments = ref([]);
const commentSearchQuery = ref('');
const commentSortBy = ref('time_desc');
const replyingToCommentId = ref(null);
const newComment = ref('');
const newCommentType = ref('comment'); 
const showMentionDropdown = ref(false);
const mentionCandidates = ref([]);
const pendingAttachments = ref([]);
const commentFileInput = ref(null);

const fetchComments = async () => {
  if (!props.selectedTask || !props.selectedTask.id) return;
  try {
    const res = await axiosClient.get(\/projects/\/tasks/\/comments\);
    comments.value = res.data.data || [];
  } catch (err) {
    if (err.response?.status !== 404) {
      console.error('L?i t?i bình lu?n:', err);
    }
    comments.value = [];
  }
};

watch(() => props.selectedTask, (newTask) => {
  if (newTask) {
    fetchComments();
    commentSearchQuery.value = '';
    commentSortBy.value = 'time_desc';
    replyingToCommentId.value = null;
    pendingAttachments.value = [];
    newComment.value = '';
  }
}, { immediate: true });

const topLevelComments = computed(() => {
  if (!comments.value) return [];
  const map = {};
  comments.value.forEach(c => {
    c.childComments = [];
    map[c.id] = c;
  });
  const roots = [];
  comments.value.forEach(c => {
    if (c.parentCommentId && map[c.parentCommentId]) {
      map[c.parentCommentId].childComments.push(c);
    } else {
      roots.push(c);
    }
  });
  return roots;
});

const filteredAndSortedComments = computed(() => {
  let result = [...topLevelComments.value];
  const q = commentSearchQuery.value.trim().toLowerCase();
  
  if (q) {
    result = result.filter(c => {
      const matchContent = c.content?.toLowerCase().includes(q);
      const matchAuthor = c.fullName?.toLowerCase().includes(q);
      const matchType = c.type === 'review' && 'dánh giá'.includes(q);
      
      const childMatch = c.childComments?.some(child => 
        child.content?.toLowerCase().includes(q) || 
        child.fullName?.toLowerCase().includes(q)
      );
      return matchContent || matchAuthor || matchType || childMatch;
    });
  }

  if (commentSortBy.value === 'time_asc') {
    result.sort((a,b) => new Date(a.createdAt) - new Date(b.createdAt));
  } else if (commentSortBy.value === 'time_desc') {
    result.sort((a,b) => new Date(b.createdAt) - new Date(a.createdAt));
  } else if (commentSortBy.value === 'assignee') {
    result.sort((a,b) => (a.fullName || '').localeCompare(b.fullName || ''));
  } else if (commentSortBy.value === 'role') {
    result.sort((a,b) => (a.role || '').localeCompare(b.role || ''));
  }

  return result;
});

const handleCommentSort = (cmd) => {
  commentSortBy.value = cmd;
};

const isImage = (file) => {
  if (!file) return false;
  const t = file.contentType || file.type || '';
  return t.startsWith('image/') || file.url?.match(/\\.(jpeg|jpg|gif|png|webp|webp-video)$/i);
};

const isImagePreview = (fileObj) => {
  if (!fileObj.file) return false;
  return fileObj.file.type.startsWith('image/');
};

const downloadFile = (file) => {
  if (!file || !file.url) return;
  const link = document.createElement('a');
  link.href = file.url;
  link.download = file.name || 'download';
  link.target = '_blank';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};

const triggerMention = () => {
  newComment.value += '@';
  handleCommentInput();
};

const selectMention = (member) => {
  const parts = newComment.value.split('@');
  parts.pop();
  newComment.value = parts.join('@') + '@' + member.fullName + ' ';
  showMentionDropdown.value = false;
  document.getElementById('comment-textarea')?.focus();
};

const handleCommentInput = () => {
  const lastAt = newComment.value.lastIndexOf('@');
  if (lastAt >= 0) {
    const afterAt = newComment.value.substring(lastAt + 1);
    const spaceIdx = afterAt.indexOf(' ');
    
    if (spaceIdx === -1) {
      const q = afterAt.toLowerCase();
      mentionCandidates.value = props.projectMembers.filter(m => m.fullName.toLowerCase().includes(q));
      showMentionDropdown.value = true;
    } else {
      showMentionDropdown.value = false;
    }
  } else {
    showMentionDropdown.value = false;
  }
};

const formatCommentWithMentions = (content) => {
  if (!content) return '';
  const regex = /@([a-zA-Z0-9_À-? ]+)/g;
  return content.replace(regex, (match, name) => {
    const isMember = props.projectMembers.some(m => m.fullName.trim() === name.trim());
    if (isMember) {
      return \<span style="color: #3b82f6; font-weight: 600; cursor: pointer;">@\</span>\;
    }
    return match;
  });
};

const handleCommentFileSelect = (e) => {
  const files = e.target.files;
  if (!files) return;
  for (let i = 0; i < files.length; i++) {
    const file = files[i];
    pendingAttachments.value.push({ file, previewUrl: file.type.startsWith('image/') ? URL.createObjectURL(file) : null });
  }
  e.target.value = '';
};

const removePendingAttachment = (idx) => {
  const att = pendingAttachments.value[idx];
  if (att && att.previewUrl) URL.revokeObjectURL(att.previewUrl);
  pendingAttachments.value.splice(idx, 1);
};

const startReply = (comment) => {
  replyingToCommentId.value = comment.id;
  newComment.value = '';
  pendingAttachments.value = [];
  nextTick(() => { document.getElementById('reply-textarea-' + comment.id)?.focus(); });
};

const cancelReply = () => {
  replyingToCommentId.value = null;
  newComment.value = '';
  pendingAttachments.value = [];
};

const submitComment = async () => {
  if (!newComment.value.trim() && pendingAttachments.value.length === 0) return;
  const taskId = props.selectedTask.id;
  try {
    const payload = {
      content: newComment.value.trim(),
      parentCommentId: replyingToCommentId.value || null,
      type: newCommentType.value
    };
    
    // Create comment
    const res = await axiosClient.post(\/projects/\/tasks/\/comments\, payload);
    const commentId = res.data.data?.id;
    
    // Handle attachments
    if (commentId && pendingAttachments.value.length > 0) {
       const fd = new FormData();
       pendingAttachments.value.forEach(obj => fd.append('Files', obj.file));
       await axiosClient.post(\/projects/\/tasks/\/comments/\/attachments\, fd, {
          headers: { 'Content-Type': 'multipart/form-data' }
       });
    }

    newComment.value = '';
    newCommentType.value = 'comment';
    replyingToCommentId.value = null;
    
    pendingAttachments.value.forEach(att => { if (att.previewUrl) URL.revokeObjectURL(att.previewUrl); });
    pendingAttachments.value = [];
    
    await fetchComments();
    emit('updateTask', props.selectedTask, 'commentCount', comments.value.length);
  } catch (error) {
    console.error('Submit comment error:', error);
    ElNotification({ title: 'L?i', message: 'Không th? dang bình lu?n', type: 'error' });
  }
};
;

modalVue += \n</script>

<style scoped>
/* Copied styles necessary for modal to look good */
.task-modal-overlay {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(9, 30, 66, 0.54);
  display: flex; align-items: flex-start; justify-content: center;
  z-index: 1000; padding: 40px; overflow-y: auto;
  backdrop-filter: blur(2px);
}
.task-modal {
  background: var(--bg-card); width: 100%; max-width: 1200px;
  border-radius: 6px; box-shadow: 0 8px 24px rgba(9,30,66,0.15);
  display: flex; flex-direction: column; overflow: hidden;
  border: 1px solid var(--border-color); color: var(--text-primary);
  min-height: 80vh;
}

/* Modals Body */
.modal-body-wrapper {
  display: flex; flex: 1; flex-direction: row; overflow: hidden;
}
.modal-main {
  flex: 2; padding: 24px 32px; overflow-y: auto;
}
.modal-sidebar {
  flex: 1; padding: 24px; border-left: 1px solid var(--border-color);
  background: var(--bg-nav); display: flex; flex-direction: column; overflow: hidden;
}

/* Header */
.modal-header {
  padding: 16px 24px; border-bottom: 1px solid var(--border-color); display: flex; justify-content: space-between; align-items: center; background: var(--bg-nav);
}
.header-left {
  display: flex; align-items: center; gap: 8px; font-size: 14px; color: var(--text-muted);
}
.m-crumb { font-weight: 500; cursor: pointer; }
.m-crumb:hover { text-decoration: underline; color: var(--text-primary); }
.separator { font-size: 10px; margin: 0 4px; }
.add-crumb {
  width: 20px; height: 20px; display: inline-flex; align-items: center; justify-content: center; background: var(--bg-hover); border-radius: 3px; cursor: pointer; color: var(--text-primary); margin-left: 4px; border: 1px solid var(--border-color);
}
.copy-crumb {
  width: 28px; height: 28px; display: inline-flex; align-items: center; justify-content: center; background: transparent; cursor: pointer; color: var(--text-muted); border-radius: 4px; margin-left: 12px;
}
.copy-crumb:hover { background: var(--bg-hover); color: var(--text-primary); }

.header-right {
  display: flex; align-items: center; gap: 16px; color: var(--text-muted);
}
.created-at { font-size: 13px; }
.btn-ai-header {
  display: flex; align-items: center; gap: 8px; background: rgba(59, 130, 246, 0.1); color: #3b82f6; font-size: 13px; padding: 6px 12px; border-radius: 4px; cursor: pointer; border: 1px solid rgba(59,130,246,0.2);
}
.btn-share {
  display: flex; align-items: center; gap: 8px; background: var(--bg-hover); color: var(--text-primary); font-size: 13px; padding: 6px 12px; border-radius: 4px; cursor: pointer; border: 1px solid var(--border-color);
}
.m-more, .m-fav, .m-pin, .m-side-toggle, .m-close {
  cursor: pointer; padding: 6px; border-radius: 4px; transition: background 0.2s;
}
.m-more:hover, .m-fav:hover, .m-pin:hover, .m-side-toggle:hover, .m-close:hover {
  background-color: var(--bg-hover); color: var(--text-primary);
}

/* Modal Content */
.task-id-row { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.status-badge-small {
  display: flex; align-items: center; gap: 6px; font-size: 12px; color: var(--text-muted); font-weight: 500; text-transform: uppercase;
}
.task-id-text { font-family: monospace; font-size: 14px; font-weight: 600; color: #94a3b8; }
.btn-ai-mini { display: flex; align-items: center; gap: 6px; color: #3b82f6; font-size: 12px; font-weight: 500; cursor: pointer; }

.task-modal-title { font-size: 28px; color: var(--text-primary); margin: 0 0 16px 0; font-weight: 600; line-height: 1.3; }

.ai-prompt-bar {
  display: flex; align-items: center; background: var(--bg-secondary); border: 1px solid var(--border-color); border-radius: 6px; padding: 8px 16px; margin-bottom: 24px; box-shadow: inset 0 2px 4px rgba(0,0,0,0.1);
}
.ai-prompt-bar .sparkle-icon { color: #8b5cf6; margin-right: 12px; font-size: 16px; }
.ai-prompt-bar input { flex: 1; min-width: 0; background: transparent; border: none; color: var(--text-primary); outline: none; font-size: 14px; }
.ai-prompt-bar input::placeholder { color: #64748b; }

.attributes-grid {
  display: grid; grid-template-columns: repeat(2, 1fr); gap: y-gap: 20px; column-gap: 40px; margin-bottom: 32px;
}
.attr-item { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; }
.attr-label { font-size: 12px; color: var(--text-secondary); font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; display: flex; align-items: center; gap: 8px; }
.attr-value { font-size: 14px; color: var(--text-primary); font-weight: 500; display: flex; align-items: center; gap: 8px; }
.attr-value.muted { color: var(--text-muted); }

.status-pill {
  display: inline-flex; align-items: center; gap: 8px; padding: 4px 10px; border-radius: 4px; font-size: 11px; font-weight: 700; cursor: pointer; text-transform: uppercase;
}
.status-pill.in-progress { background: rgba(59, 130, 246, 0.15); color: #60a5fa; border: 1px solid rgba(59, 130, 246, 0.3); }

.content-section { display: flex; flex-direction: column; gap: 12px; }
.section-link { color: var(--text-secondary); font-size: 14px; font-weight: 500; cursor: pointer; display: flex; align-items: center; gap: 8px; padding: 8px; border-radius: 4px; border: 1px dashed var(--border-color); background: var(--bg-nav); }
.section-link:hover { background: var(--bg-hover); color: var(--text-primary); border-color: #64748b; }
.ai-link { border-color: rgba(139, 92, 246, 0.3); color: #a78bfa; background: rgba(139, 92, 246, 0.05); }

/* Right Sidebar (Comments) */
.sidebar-header {
  display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;
}
.sidebar-header h2 { font-size: 16px; margin: 0; color: var(--text-primary); font-weight: 600; }
.header-tools { display: flex; align-items: center; gap: 16px; color: var(--text-secondary); font-size: 14px; }

.activity-scroll { flex: 1; overflow-y: auto; padding-right: 8px; margin-bottom: 16px; display: flex; flex-direction: column; gap: 20px; }
.comment-card { display: flex; flex-direction: column; }
.c-head { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
.avatar-sm { width: 32px; height: 32px; border-radius: 50%; background-color: #1d4ed8; color: white; display: flex; align-items: center; justify-content: center; font-size: 14px; font-weight: 600; }
.c-user { font-size: 14px; font-weight: 600; color: var(--text-primary); display: flex; align-items: baseline; gap: 8px; }
.c-time { font-size: 12px; font-weight: 400; color: var(--text-muted); }
.c-body { font-size: 14px; line-height: 1.5; color: var(--text-secondary); margin-left: 44px; margin-bottom: 8px; }
.c-foot { display: flex; align-items: center; gap: 16px; margin-left: 44px; color: var(--text-muted); font-size: 12px; font-weight: 500; }
.c-actions { display: flex; gap: 12px; cursor: pointer; }
.c-rep { cursor: pointer; }
.c-rep:hover, .c-actions i:hover { color: var(--text-primary); }

.replies-container { margin-left: 44px; margin-top: 16px; padding-left: 16px; border-left: 2px solid var(--border-color); display: flex; flex-direction: column; gap: 16px; }

.inline-reply-box { display: flex; gap: 12px; align-items: flex-start; }
.inline-input-wrapper {
  background: var(--bg-layout); border: 1px solid var(--border-color); border-radius: 6px; padding: 8px 12px; display: flex; flex-direction: column; gap: 8px; flex: 1;
}
.inline-input-wrapper textarea { width: 100%; border: none; background: transparent; color: var(--text-primary); resize: none; min-height: 40px; font-family: inherit; font-size: 13px; outline: none; }
.inline-actions { display: flex; justify-content: flex-end; gap: 12px; font-size: 14px; color: var(--text-muted); align-items: center; }
.inline-actions i { cursor: pointer; }
.inline-actions i.send-enabled { color: #3b82f6; }
.inline-actions i.cancel-btn:hover { color: #ef4444; }

.activity-input { margin-top: auto; }
.input-container { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 6px; display: flex; flex-direction: column; padding: 12px; gap: 12px; box-shadow: 0 -4px 12px rgba(0,0,0,0.2); }
.input-container textarea { width: 100%; border: none; background: transparent; color: var(--text-primary); resize: none; min-height: 50px; font-family: inherit; font-size: 14px; outline: none; }
.input-container textarea::placeholder { color: var(--text-muted); }

.input-actions-bar { display: flex; justify-content: space-between; align-items: center; color: var(--text-secondary); font-size: 16px; }
.bar-left { display: flex; gap: 16px; align-items: center; cursor: pointer; }
.bar-left i:hover { color: var(--text-primary); }
.bar-left .ai { color: #a78bfa; }
.bar-right { display: flex; gap: 12px; align-items: center; cursor: pointer; }
.bar-right .fa-paper-plane { color: #64748b; }
.bar-right .fa-paper-plane.send-enabled { color: #3b82f6; }

.btn-comment-type { background: var(--bg-hover); color: var(--text-primary); border: 1px solid var(--border-color); padding: 4px 8px; border-radius: 4px; font-size: 12px; cursor: pointer; display: flex; align-items: center; gap: 6px; }
</style>
;

fs.writeFileSync(modalFile, modalVue);
console.log('TaskDetailModal.vue created!');

// Now replace in SpaceSummary.vue
let newSummary = content.substring(0, startIndex) +
\      <!-- Task Detail Modal Component -->
      <TaskDetailModal 
        v-if="showTaskModal" 
        :selectedTask="selectedTask" 
        :projectId="projectId"
        :projectMembers="projectMembers"
        :currentUser="currentUser"
        @close="showTaskModal = false"
        @updateTask="updateTaskField"
        @open-task="openTaskDetail"
        @create-subtask="createSubtask"
      />
\ + content.substring(endIndex);

fs.writeFileSync(spaceFile, newSummary);
console.log('SpaceSummary.vue template updated!');

