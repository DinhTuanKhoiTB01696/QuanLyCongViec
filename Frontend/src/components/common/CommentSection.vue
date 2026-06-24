<template>
  <div class="comment-section">
    <h3>Bình luận</h3>
    <div class="comments-list">
      <div v-for="comment in comments" :key="comment.id" class="comment-item">
        <div class="comment-avatar">{{ comment.avatarUrl || comment.fullName?.substring(0,2).toUpperCase() || 'U' }}</div>
        <div class="comment-body">
          <div class="comment-header">
            <span class="comment-author">{{ comment.fullName }}</span>
            <span class="comment-time">{{ formatDate(comment.createdAt) }}</span>
          </div>
          <div class="comment-content" v-html="comment.content"></div>
        </div>
      </div>
      
      <div v-if="comments.length === 0 && !isLoading" class="empty-comments">
        Chưa có bình luận nào.
      </div>
      <div v-if="isLoading" class="loading-state">
        <div class="loader-spinner"></div>
      </div>
    </div>
    
    <div class="comment-input-area">
      <textarea 
        v-model="newComment" 
        placeholder="Viết bình luận..." 
        rows="3"
        :disabled="isSubmitting"
      ></textarea>
      <div class="comment-actions">
        <button 
          class="primary-btn" 
          @click="submitComment" 
          :disabled="!newComment.trim() || isSubmitting"
        >
          {{ isSubmitting ? 'Đang gửi...' : 'Gửi' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  entityId: { type: String, required: true },
  entityType: { type: String, required: true }
})

const comments = ref([])
const newComment = ref('')
const isLoading = ref(false)
const isSubmitting = ref(false)

const loadComments = async () => {
  isLoading.value = true
  try {
    const res = await axiosClient.get(`/comments/${props.entityType}/${props.entityId}`)
    comments.value = res.data?.data || []
  } catch (error) {
    console.error('Failed to load comments', error)
  } finally {
    isLoading.value = false
  }
}

const submitComment = async () => {
  if (!newComment.value.trim()) return
  
  isSubmitting.value = true
  try {
    const formData = new FormData()
    formData.append('content', newComment.value)
    
    const res = await axiosClient.post(`/comments/${props.entityType}/${props.entityId}`, formData)
    if (res.data?.data) {
      comments.value.push(res.data.data)
      newComment.value = ''
    }
  } catch (error) {
    console.error('Failed to submit comment', error)
  } finally {
    isSubmitting.value = false
  }
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('vi-VN', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit'
  }).format(date)
}

onMounted(() => {
  loadComments()
})
</script>

<style scoped>
.comment-section {
  margin-top: 24px;
}

.comment-section h3 {
  font-size: 16px;
  color: #172B4D;
  margin-bottom: 16px;
}

.comments-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 24px;
}

.comment-item {
  display: flex;
  gap: 12px;
}

.comment-avatar {
  width: 32px;
  height: 32px;
  background-color: #0052CC;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: bold;
  flex-shrink: 0;
}

.comment-body {
  flex: 1;
}

.comment-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 4px;
}

.comment-author {
  font-weight: 600;
  color: #172B4D;
  font-size: 14px;
}

.comment-time {
  color: #5E6C84;
  font-size: 12px;
}

.comment-content {
  color: #172B4D;
  font-size: 14px;
  line-height: 1.5;
  background: #FAFBFC;
  padding: 12px;
  border-radius: 3px;
  border: 1px solid #DFE1E6;
}

.empty-comments {
  color: #5E6C84;
  font-style: italic;
  font-size: 14px;
}

.comment-input-area {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  resize: vertical;
  font-family: inherit;
  font-size: 14px;
}

textarea:focus {
  outline: none;
  border-color: #4C9AFF;
}

.comment-actions {
  display: flex;
  justify-content: flex-end;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 6px 16px;
  border-radius: 3px;
  font-weight: 500;
  cursor: pointer;
}

.primary-btn:hover:not(:disabled) {
  background-color: #0047B3;
}

.primary-btn:disabled {
  background-color: #091E420A;
  color: #A5ADBA;
  cursor: not-allowed;
}

.loading-state {
  display: flex;
  justify-content: center;
  padding: 16px;
}

.loader-spinner {
  width: 24px;
  height: 24px;
  border: 2px solid #DFE1E6;
  border-top-color: #0052CC;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
