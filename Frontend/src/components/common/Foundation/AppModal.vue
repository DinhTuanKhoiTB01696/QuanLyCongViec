<template>
  <el-dialog
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :title="title"
    :width="width"
    :destroy-on-close="destroyOnClose"
    :close-on-click-modal="false"
    class="sprinta-app-modal"
    @close="$emit('close')"
    @closed="$emit('closed')"
    append-to-body
  >
    <template #header v-if="$slots.header">
      <slot name="header"></slot>
    </template>
    
    <div class="sprinta-app-modal-body">
      <slot></slot>
    </div>

    <template #footer v-if="showFooter || $slots.footer">
      <span class="dialog-footer sprint-app-modal-footer">
        <slot name="footer">
          <el-button @click="handleCancel" class="cancel-btn">{{ cancelText }}</el-button>
          <el-button type="primary" @click="handleConfirm" :loading="loading" class="primary-btn">
            {{ confirmText }}
          </el-button>
        </slot>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  title: {
    type: String,
    default: ''
  },
  width: {
    type: String,
    default: '500px'
  },
  loading: {
    type: Boolean,
    default: false
  },
  confirmText: {
    type: String,
    default: 'Xác nhận'
  },
  cancelText: {
    type: String,
    default: 'Hủy'
  },
  showFooter: {
    type: Boolean,
    default: true
  },
  destroyOnClose: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel', 'close', 'closed'])

const handleConfirm = () => {
  emit('confirm')
}

const handleCancel = () => {
  emit('cancel')
  emit('update:modelValue', false)
}
</script>

<style>
/* Global overrides for this specific modal instance */
.sprinta-app-modal {
  border-radius: var(--sp-radius-lg, 8px) !important;
  box-shadow: 0 8px 16px -4px rgba(9, 30, 66, 0.25) !important;
  padding: 0 !important;
}

.sprinta-app-modal .el-dialog__header {
  padding: 20px 24px;
  margin-right: 0;
  border-bottom: 1px solid var(--sp-border-color, #DFE1E6);
  font-size: 20px;
  font-weight: 500;
  color: var(--sp-text-primary, #172B4D);
}

.sprinta-app-modal .el-dialog__body {
  padding: 24px;
  color: var(--sp-text-primary, #172B4D);
}

.sprinta-app-modal .el-dialog__footer {
  padding: 16px 24px;
  border-top: 1px solid var(--sp-border-color, #DFE1E6);
  margin-top: 0;
}

.sprinta-app-modal-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.sprint-app-modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.sprint-app-modal-footer .cancel-btn {
  background: transparent;
  color: var(--sp-text-secondary, #5E6C84);
  border: none;
  font-weight: 500;
}

.sprint-app-modal-footer .cancel-btn:hover {
  background: rgba(9, 30, 66, 0.08);
}

.sprint-app-modal-footer .primary-btn {
  background-color: var(--sp-brand-primary, #0052CC);
  color: white;
  border: none;
  font-weight: 500;
}

.sprint-app-modal-footer .primary-btn:hover {
  background-color: #0047B3;
}
</style>
