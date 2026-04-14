<template>
<transition name="fade">
        <div class="task-modal-overlay" v-if="showTaskModal" @click.self="showTaskModal = false">
          <div class="task-modal">
            <!-- Modal Header -->
            <header class="modal-header">
              <div class="header-left">
                <i class="fa-solid fa-table-columns"></i>
                <span class="m-crumb">Không gian nhóm</span>
                <i class="fa-solid fa-chevron-right separator"></i>
                <span class="m-crumb">Dự án</span>
                <i class="fa-solid fa-chevron-right separator"></i>
                <span class="m-crumb current">Dự án 1</span>
                <i class="fa-solid fa-plus add-crumb"></i>
                <i class="fa-solid fa-copy copy-crumb"></i>
              </div>
              <div class="header-right">
                <span class="created-at">Đã tạo vào 18 tháng 3</span>
                <div class="btn-ai-header"><i class="fa-solid fa-microchip"></i> Hỏi AI</div>
                <div class="btn-share"><i class="fa-solid fa-share-nodes"></i> Chia sẻ</div>
                <i class="fa-solid fa-ellipsis m-more"></i>
                <i class="fa-solid fa-star m-fav"></i>
                <i class="fa-solid fa-thumbtack m-pin"></i>
                <i class="fa-solid fa-chevron-right m-side-toggle"></i>
                <i class="fa-solid fa-xmark m-close" @click="showTaskModal = false"></i>
              </div>
            </header>

            <div class="modal-body-wrapper">
              <!-- Left Content -->
              <div class="modal-main">
                <div class="task-id-row">
                  <div class="status-badge-small"><i class="fa-solid fa-circle"></i> Trạng thái</div>
                  <span class="task-id-text">SprintA-123</span>
                  <div class="btn-ai-mini"><i class="fa-solid fa-wand-magic-sparkles"></i> Hỏi AI</div>
                </div>

                <h1 class="task-modal-title">{{ selectedTask?.title }}</h1>

                <div class="ai-prompt-bar">
                   <div class="sparkle-icon"><i class="fa-solid fa-wand-magic-sparkles"></i></div>
                   <input type="text" placeholder="Yêu cầu Brain viết mô tả, tạo công việc con hoặc tìm các công việc tương tự" />
                </div>

                <!-- Attributes Grid -->
                <div class="attributes-grid">
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-solid fa-circle-half-stroke"></i> Trạng thái</div>
                    <div class="attr-value">
                      <div class="status-pill in-progress">
                        <i class="fa-solid fa-circle-half-stroke"></i> {{ selectedTask?.statusName.toUpperCase() }} <i class="fa-solid fa-chevron-down"></i>
                      </div>
                    </div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-regular fa-user"></i> Người báo cáo</div>
                    <div class="attr-value">{{ selectedTask?.reporterName }}</div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-regular fa-calendar"></i> Ngày tháng</div>
                    <div class="attr-value">
                      <span v-if="selectedTask?.plannedStartDate">{{ formatDate(selectedTask.plannedStartDate) }}</span>
                      <i class="fa-solid fa-arrow-right" v-if="selectedTask?.plannedStartDate && selectedTask?.plannedEndDate"></i>
                      <span v-if="selectedTask?.plannedEndDate">{{ formatDate(selectedTask.plannedEndDate) }}</span>
                      <span v-if="!selectedTask?.plannedStartDate && !selectedTask?.plannedEndDate">Trống</span>
                    </div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-regular fa-flag"></i> Độ ưu tiên</div>
                    <div class="attr-value">{{ selectedTask?.priority || 'Trống' }}</div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-solid fa-user-check"></i> Người thực hiện</div>
                    <div class="attr-value">
                      <el-dropdown trigger="click" @command="(val) => updateTaskField(selectedTask, 'assignedUserId', val)">
                        <span class="cursor-pointer" v-if="selectedTask?.assigneeName">{{ selectedTask.assigneeName }}</span>
                        <span class="cursor-pointer muted" v-else>Chưa phân công</span>
                        <template #dropdown>
                          <el-dropdown-menu>
                             <el-dropdown-item :command="null">Chưa phân công</el-dropdown-item>
                             <el-dropdown-item v-for="member in projectMembers" :key="member.userId" :command="member.userId">
                               {{ member.fullName }}
                             </el-dropdown-item>
                          </el-dropdown-menu>
                        </template>
                      </el-dropdown>
                    </div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-solid fa-calendar-day"></i> Ngày hết hạn</div>
                    <div class="attr-value">
                       <el-date-picker
                        v-model="selectedTask.dueDate"
                        type="date"
                        placeholder="Chọn ngày"
                        size="small"
                        format="YYYY-MM-DD"
                        value-format="YYYY-MM-DD"
                        @change="(val) => updateTaskField(selectedTask, 'dueDate', val)"
                        class="inline-date-picker"
                      />
                    </div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-regular fa-star"></i> Story Points</div>
                    <div class="attr-value">{{ selectedTask?.storyPoints || '0' }}</div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-regular fa-clock"></i> Theo dõi thời gian</div>
                    <div class="attr-value"><i class="fa-regular fa-circle-play"></i> Thêm thời gian</div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-solid fa-tag"></i> Thẻ</div>
                    <div class="attr-value muted">Trống</div>
                  </div>
                  <div class="attr-item">
                    <div class="attr-label"><i class="fa-solid fa-share-nodes"></i> Mối quan hệ</div>
                    <div class="attr-value muted">Trống</div>
                  </div>
                </div>

                <!-- Description Areas -->
                <div class="content-section">
                  <div class="section-link"><i class="fa-regular fa-file-lines"></i> Thêm mô tả</div>
                  <div class="section-link ai-link"><i class="fa-solid fa-wand-magic-sparkles"></i> Viết bằng AI</div>
                </div>

                <div class="fields-section">
                  <h3>Thêm trường dữ liệu</h3>
                  <button class="add-field-btn"><i class="fa-solid fa-plus"></i> Tạo một trường trong danh sách này</button>
                </div>
              </div>

              <!-- Right Activity Panel -->
              <div class="modal-sidebar">
                <div class="sidebar-header">
                  <h2>Hoạt động</h2>
                  <div class="header-tools">
                    <i class="fa-solid fa-magnifying-glass"></i>
                    <i class="fa-solid fa-arrow-down-wide-short"></i>
                    <i class="fa-solid fa-comments"></i> 1
                    <i class="fa-solid fa-bars-staggered"></i>
                  </div>
                </div>

                <div class="activity-scroll">
                  <div class="comment-card" v-for="c in topLevelComments" :key="c.id">
                    <div class="c-head">
                      <div class="avatar-sm">{{ c.avatar || 'U' }}</div>
                      <div class="c-user">{{ c.fullName }} <span class="c-time">{{ formatDate(c.createdAt) }}</span></div>
                    </div>
                    <div class="c-body">{{ c.content }}</div>
                    <div class="c-foot">
                       <div class="c-actions">
                         <i class="fa-regular fa-thumbs-up"></i>
                         <i class="fa-regular fa-face-smile"></i>
                       </div>
                       <div class="c-rep" @click="startReply(c)">Trả lời</div>
                    </div>
                    <div class="replies-container" v-if="(c.childComments && c.childComments.length > 0) || replyingToCommentId === c.id">
                      <div class="comment-card reply-card" v-for="reply in c.childComments" :key="reply.id">
                        <div class="c-head">
                          <div class="avatar-sm" style="width: 20px; height: 20px; font-size: 9px;">{{ reply.avatar || 'U' }}</div>
                          <div class="c-user" style="font-size: 12px;">{{ reply.fullName }} <span class="c-time">{{ formatDate(reply.createdAt) }}</span></div>
                        </div>
                        <div class="c-body" style="font-size: 13px;">{{ reply.content }}</div>
                      </div>
                      
                      <div class="inline-reply-box" v-if="replyingToCommentId === c.id">
                        <div class="avatar-sm" style="width: 20px; height: 20px; font-size: 9px; align-self: flex-start; margin-top: 6px;">{{ currentUser?.name ? currentUser.name.charAt(0).toUpperCase() : 'U' }}</div>
                        <div class="inline-input-wrapper">
                           <textarea 
                              :id="'reply-textarea-' + c.id" 
                              placeholder="Viết phản hồi công khai..." 
                              v-model="newComment" 
                              @keyup.enter.ctrl="submitComment"
                           ></textarea>
                           <div class="inline-actions">
                             <i class="fa-solid fa-paper-plane" :class="{ 'send-enabled': !!newComment }" @click="submitComment"></i>
                             <i class="fa-solid fa-xmark cancel-btn" @click="cancelReply" title="Hủy"></i>
                           </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="activity-input" v-show="!replyingToCommentId">
                  <div class="input-container">
                    <textarea id="comment-textarea" placeholder="Viết bình luận..." v-model="newComment" @keyup.enter.ctrl="submitComment"></textarea>
                    <div class="input-actions-bar">
                      <div class="bar-left">
                        <i class="fa-solid fa-plus"></i>
                        <button class="btn-comment-type">Bình luận <i class="fa-solid fa-chevron-down"></i></button>
                        <i class="fa-solid fa-wand-magic-sparkles ai"></i>
                        <i class="fa-solid fa-at"></i>
                        <i class="fa-solid fa-paperclip" @click="triggerFileUpload"></i>
                        <i class="fa-solid fa-at"></i>
                        <i class="fa-regular fa-face-smile"></i>
                        <i class="fa-solid fa-ellipsis"></i>
                      </div>
                      <div class="bar-right">
                        <i class="fa-solid fa-paper-plane" :class="{ 'send-enabled': !!newComment }" @click="submitComment"></i>
                        <i class="fa-solid fa-chevron-down"></i>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </transition>
</template>


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
    const res = await axiosClient.get(`/projects/${props.projectId}/tasks/${props.selectedTask.id}/comments`);
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
        await axiosClient.post(`/projects/${props.projectId}/tasks/${props.selectedTask.id}/comments`, payload);
        newComment.value = '';
        replyingToCommentId.value = null;
        fetchComments();
    } catch(e) {}
};
</script>
