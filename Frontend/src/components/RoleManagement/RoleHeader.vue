<template>
  <el-row type="flex" justify="space-between" align="middle" style="width: 100%;">
    <el-col :span="18">
      <div style="display: flex; align-items: center; gap: 12px; margin-bottom: 8px;">
        <el-text tag="h1" style="font-size: 24px; font-weight: 600; color: var(--el-text-color-primary); margin: 0; line-height: 1.2;">
          {{ role.name }}
        </el-text>
        <el-tag v-if="role.isProtected" type="warning" effect="dark" size="small" style="font-weight: 600;">System Protected</el-tag>
        <el-tag v-else type="info" effect="plain" size="small" style="font-weight: 600;">Custom Role</el-tag>
      </div>
      
      <el-text type="info" style="display: block; margin-bottom: 16px; font-size: 14px;">
        {{ role.description || 'No description provided.' }}
      </el-text>
      
      <el-space :size="24">
        <el-text type="info" size="small">
          <el-icon style="vertical-align: middle; margin-right: 4px;"><User /></el-icon> 
          <span style="vertical-align: middle;">{{ userCount }} Assigned Users</span>
        </el-text>
        <el-text type="info" size="small">
          <el-icon style="vertical-align: middle; margin-right: 4px;"><Key /></el-icon> 
          <span style="vertical-align: middle;">{{ role.permissionIds?.length || 0 }} Active Permissions</span>
        </el-text>
      </el-space>
    </el-col>
    
    <el-col :span="6" style="text-align: right; display: flex; gap: 12px; justify-content: flex-end;">
      <el-button disabled title="Preview not supported by current API">Preview</el-button>
      <el-button @click="$emit('duplicate')">
        <el-icon style="margin-right: 6px;"><CopyDocument /></el-icon> Duplicate
      </el-button>
    </el-col>
  </el-row>
</template>

<script setup>
import { User, Key, CopyDocument } from '@element-plus/icons-vue'

defineProps({
  role: { type: Object, required: true },
  userCount: { type: Number, default: 0 }
})
defineEmits(['duplicate'])
</script>
