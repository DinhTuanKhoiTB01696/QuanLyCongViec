<template>
  <div class="sprinta-select" :style="{ width: width }">
    <el-select
      v-model="internalValue"
      :placeholder="placeholder"
      :clearable="clearable"
      :filterable="filterable"
      :loading="loading"
      :teleported="true"
      class="sprinta-el-select"
    >
      <el-option
        v-for="item in options"
        :key="item[valueKey]"
        :label="item[labelKey]"
        :value="item[valueKey]"
      >
        <slot name="option" :item="item" />
      </el-option>
    </el-select>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number, Boolean, Array, Object],
    default: null
  },
  options: {
    type: Array,
    default: () => []
  },
  labelKey: {
    type: String,
    default: 'label'
  },
  valueKey: {
    type: String,
    default: 'value'
  },
  placeholder: {
    type: String,
    default: 'Chọn...'
  },
  clearable: {
    type: Boolean,
    default: true
  },
  filterable: {
    type: Boolean,
    default: false
  },
  loading: {
    type: Boolean,
    default: false
  },
  width: {
    type: String,
    default: '200px'
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const internalValue = computed({
  get: () => props.modelValue,
  set: (val) => {
    emit('update:modelValue', val)
    emit('change', val)
  }
})
</script>

<style scoped>
.sprinta-select {
  display: inline-block;
}
:deep(.sprinta-el-select .el-input__wrapper) {
  height: var(--sp-control-height, 36px);
  border-radius: var(--sp-control-radius, 4px);
  box-shadow: 0 0 0 1px var(--sp-border-color, #DFE1E6) inset;
}
:deep(.sprinta-el-select .el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #A5ADBA inset;
}
:deep(.sprinta-el-select .el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px #4C9AFF inset !important;
}
</style>
