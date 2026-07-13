<template>
  <div class="sprinta-search-input" :style="{ width: width }">
    <el-input
      v-model="internalValue"
      :placeholder="placeholder"
      :clearable="clearable"
      class="sprinta-el-input"
    >
      <template #prefix>
        <i class="fa-solid fa-search" style="color: var(--sp-text-secondary);"></i>
      </template>
    </el-input>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: 'Tìm kiếm...'
  },
  clearable: {
    type: Boolean,
    default: true
  },
  width: {
    type: String,
    default: '240px'
  }
})

const emit = defineEmits(['update:modelValue'])

const internalValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})
</script>

<style scoped>
.sprinta-search-input {
  display: inline-block;
}
:deep(.sprinta-el-input .el-input__wrapper) {
  height: var(--sp-control-height, 36px);
  border-radius: var(--sp-control-radius, 4px);
  box-shadow: 0 0 0 1px var(--sp-border-color, #DFE1E6) inset;
}
:deep(.sprinta-el-input .el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #A5ADBA inset;
}
:deep(.sprinta-el-input .el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px #4C9AFF inset;
}
</style>
