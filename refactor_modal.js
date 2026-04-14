const fs = require('fs');
const path = require('path');

const modalFile = path.join('d:', 'A', 'QuanLyCongViec', 'Frontend', 'src', 'components', 'TaskDetailModal.vue');
const templateStr = fs.readFileSync('modalTemplate.html', 'utf8');

const scriptStr = `
<script setup>
import { ref, watch, computed } from 'vue';
import { ElMessage, ElNotification } from 'element-plus';
import axiosClient from '@/api/axiosClient';

const props = defineProps({
  selectedTask: { type: Object, default: null },
  projectId: { type: [String, Number], required: true },
  projectMembers: { type: Array, default: () => [] },
  currentUser: { type: Object, default: () => ({}) }
});

const emit = defineEmits(['updateTask', 'close', 'open-task', 'create-subtask', 'refresh-tasks']);

const showTaskModal = ref(true);

watch(showTaskModal, (val) => {
  if (!val) emit('close');
});

const formatDate = (dateStr) => {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN');
};

const updateTaskField = (task, field, value) => {
  emit('updateTask', task, field, value);
};

const openTaskDetail = (task) => emit('open-task', task);
const createSubtask = (task) => emit('create-subtask', task);

// Comments logic
const comments = ref([]);
const replyingToCommentId = ref(null);
const newComment = ref('');
const pendingAttachments = ref([]);
const commentFileInput = ref(null);

const fetchComments = async () => {
  if (!props.selectedTask || !props.selectedTask.id) return;
  try {
    const res = await axiosClient.get(\`/projects/\${props.projectId}/tasks/\${props.selectedTask.id}/comments\`);
    comments.value = res.data?.data || [];
  } catch (err) { }
};

watch(() => props.selectedTask, (newTask) => {
  if (newTask) {
    fetchComments();
    replyingToCommentId.value = null;
    newComment.value = '';
    showTaskModal.value = true;
  }
}, { immediate: true });

const topLevelComments = computed(() => {
  if (!comments.value) return [];
  const map = {};
  comments.value.forEach(c => { c.childComments = []; map[c.id] = c; });
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

const triggerFileUpload = () => { if (commentFileInput.value) commentFileInput.value.click(); };
const startReply = (c) => { replyingToCommentId.value = c.id; newComment.value = ''; pendingAttachments.value = []; };
const cancelReply = () => { replyingToCommentId.value = null; newComment.value = ''; pendingAttachments.value = []; };
const submitComment = async () => {
    if (!newComment.value.trim()) return;
    try {
        const payload = { content: newComment.value.trim(), parentCommentId: replyingToCommentId.value || null };
        await axiosClient.post(\`/projects/\${props.projectId}/tasks/\${props.selectedTask.id}/comments\`, payload);
        newComment.value = '';
        replyingToCommentId.value = null;
        fetchComments();
    } catch(e) {}
};
</script>
`;

fs.writeFileSync(modalFile, templateStr + '\n' + scriptStr);
console.log('TaskDetailModal.vue created successfully');
