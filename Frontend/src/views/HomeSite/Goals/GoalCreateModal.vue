<template>
  <Teleport to="body">
    <div v-if="modelValue" class="goal-modal-overlay" @click.self="close">
      <div class="goal-modal-content">
        <div class="goal-modal-header">
          <h2>
            <Target class="w-4 h-4 header-icon" />
            Tạo mục tiêu
          </h2>
        </div>

        <div class="goal-modal-body">
          <p class="form-hint">Các trường bắt buộc được đánh dấu sao <span class="required">*</span></p>

          <div class="form-group">
            <label>Tên <span class="required">*</span></label>
            <input
              type="text"
              v-model="newGoal.title"
              @blur="isTitleTouched = true"
              :class="{ 'error-input': isTitleTouched && !newGoal.title }"
            />
            <div v-if="isTitleTouched && !newGoal.title" class="error-message">
              <i class="fa-solid fa-circle-exclamation"></i> Bạn phải đặt tên mục tiêu
            </div>
          </div>

          <div class="form-group">
            <label>Loại <span class="required">*</span></label>
            <div class="readonly-field">
              <Target class="w-4 h-4 readonly-icon-left" />
              <span>Objective</span>
              <ChevronDown class="w-4 h-4 readonly-icon-right" />
            </div>
          </div>

          <div class="form-group">
            <label>Ngày bắt đầu (Ngày mục tiêu) <span class="required">*</span></label>
            <el-date-picker
              v-model="newGoal.startDate"
              type="date"
              placeholder="Chọn ngày"
              format="DD/MM/YYYY"
              value-format="YYYY-MM-DD"
              class="goal-date-picker"
              popper-class="goal-create-date-popper"
              :teleported="true"
              :class="{ 'error-input': isDateTouched && !newGoal.startDate }"
              @change="isDateTouched = true"
            />
            <div v-if="isDateTouched && !newGoal.startDate" class="error-message">
              <i class="fa-solid fa-circle-exclamation"></i> Bạn phải chọn ngày bắt đầu
            </div>
          </div>

          <div class="form-group owner-field">
            <label>Chủ sở hữu <span class="required">*</span></label>
            <div class="owner-input-wrapper" @click="isOwnerDropdownOpen = !isOwnerDropdownOpen">
              <UserAvatar :user="newGoal.owner" size="sm" />
              <span>{{ newGoal.owner?.fullName || newGoal.owner?.email || 'Chọn chủ sở hữu' }}</span>
            </div>

            <div v-if="isOwnerDropdownOpen" class="owner-dropdown">
              <div v-for="user in users" :key="user.id" class="owner-option" @click="selectOwner(user)">
                <UserAvatar :user="user" size="sm" />
                <span>{{ user.fullName || user.email }}</span>
              </div>
            </div>
          </div>
        </div>

        <div class="goal-modal-footer">
          <button class="cancel-btn" @click="close">Hủy</button>
          <button class="primary-btn" @click="submitCreateGoal">Tạo</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Target, ChevronDown } from 'lucide-vue-next'
import { useGoalStore } from '@/store/useGoalStore'
import UserAvatar from '@/components/common/UserAvatar.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  users: {
    type: Array,
    default: () => []
  },
  currentUser: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['update:modelValue', 'created'])
const goalStore = useGoalStore()

const isTitleTouched = ref(false)
const isDateTouched = ref(false)
const isOwnerDropdownOpen = ref(false)
const newGoal = ref({
  title: '',
  type: 'Objective',
  startDate: '',
  owner: null,
  status: 'ĐANG CHỜ XỬ LÝ'
})

const resetForm = () => {
  newGoal.value = {
    title: '',
    type: 'Objective',
    startDate: '',
    owner: props.currentUser,
    status: 'ĐANG CHỜ XỬ LÝ'
  }
  isTitleTouched.value = false
  isDateTouched.value = false
  isOwnerDropdownOpen.value = false
}

watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) resetForm()
  },
  { immediate: true }
)

const close = () => {
  emit('update:modelValue', false)
}

const selectOwner = (user) => {
  newGoal.value.owner = user
  isOwnerDropdownOpen.value = false
}

const submitCreateGoal = async () => {
  isTitleTouched.value = true
  isDateTouched.value = true
  if (!newGoal.value.title || !newGoal.value.startDate) return

  const createdGoal = await goalStore.createGoal({
    title: newGoal.value.title,
    status: newGoal.value.status,
    ownerId: newGoal.value.owner?.id,
    type: newGoal.value.type,
    startDate: newGoal.value.startDate
  })
  await goalStore.fetchGoals()
  emit('created', createdGoal)
  close()
}
</script>

<style scoped>
.goal-modal-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(9, 30, 66, 0.54);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 3000;
}

:global(.goal-create-date-popper) {
  z-index: 4001 !important;
}

.goal-modal-content {
  width: 500px;
  max-width: calc(100vw - 32px);
  background-color: #FFFFFF;
  border-radius: 3px;
  box-shadow: 0 8px 16px -4px rgba(9, 30, 66, 0.25);
}

.goal-modal-header {
  padding: 20px 24px 0;
}

.goal-modal-header h2 {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0;
  font-size: 20px;
  font-weight: 500;
  color: #172B4D;
}

.header-icon {
  color: #6B778C;
}

.goal-modal-body {
  padding: 12px 24px 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-hint {
  font-size: 11px;
  color: #6B778C;
  margin: 0 0 16px;
}

.form-group label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
  margin-bottom: 8px;
}

.form-group input {
  width: 100%;
  padding: 8px 12px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  box-sizing: border-box;
  outline: none;
}

.form-group input:focus {
  border-color: #4C9AFF;
}

.required {
  color: #DE350B;
}

.error-input {
  border-color: #DE350B !important;
}

.error-message {
  color: #DE350B;
  font-size: 11px;
  margin-top: 4px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.readonly-field {
  position: relative;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  background: #FAFBFC;
  padding: 8px 36px;
  color: #172B4D;
  cursor: not-allowed;
  opacity: 0.8;
  display: flex;
  align-items: center;
}

.readonly-icon-left {
  position: absolute;
  left: 10px;
  color: #6B778C;
}

.readonly-icon-right {
  position: absolute;
  right: 10px;
  color: #6B778C;
  pointer-events: none;
}

.goal-date-picker {
  width: 100%;
  height: 36px;
}

.owner-field {
  position: relative;
}

.owner-input-wrapper {
  position: relative;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  padding: 6px 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  background: white;
}

.owner-input-wrapper span,
.owner-option span {
  font-size: 14px;
  color: #172B4D;
}

.owner-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  margin-top: 4px;
  background: white;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  width: 100%;
  z-index: 1;
  max-height: 200px;
  overflow-y: auto;
}

.owner-option {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  transition: background 0.1s;
}

.owner-option:hover {
  background: #FAFBFC;
}

.goal-modal-footer {
  padding: 16px 24px;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  border-top: 1px solid #DFE1E6;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
}

.primary-btn:hover {
  background-color: #0047B3;
}

.cancel-btn {
  background: transparent;
  color: #5E6C84;
  border: none;
  padding: 8px 12px;
  border-radius: 3px;
  font-weight: 500;
  cursor: pointer;
}

.cancel-btn:hover {
  background: rgba(9, 30, 66, 0.08);
}
</style>
