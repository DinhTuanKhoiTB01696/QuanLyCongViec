<template>
  <div style="display: flex; flex-direction: column; height: 100%;">
    <div style="padding: 24px 20px 16px;">
      <el-text tag="h2" style="font-size: 18px; font-weight: 600; margin: 0 0 4px;">Role Management</el-text>
      <el-text type="info" size="small">{{ roles.length }} Roles • {{ roles.filter(r => r.isProtected).length }} System</el-text>
      
      <div style="margin-top: 16px;">
        <el-input 
          v-model="searchQuery" 
          placeholder="Search roles..." 
          prefix-icon="Search"
          clearable
        />
      </div>
    </div>
    
    <el-scrollbar style="flex: 1;">
      <el-menu 
        :default-active="selectedRoleId"
        @select="id => $emit('select-role', roles.find(r => r.id === id))"
        style="border-right: none;"
      >
        <template v-if="filteredSystemRoles.length > 0">
          <el-menu-item-group>
            <template #title>
              <el-text type="info" size="small" style="font-weight: 600; letter-spacing: 0.05em;">SYSTEM ROLES</el-text>
            </template>
            <el-menu-item 
              v-for="role in filteredSystemRoles" 
              :key="role.id" 
              :index="role.id"
            >
              <el-icon><Lock /></el-icon>
              <span style="flex: 1; overflow: hidden; text-overflow: ellipsis; padding-left: 4px;">{{ role.name }}</span>
              <el-dropdown v-if="selectedRoleId === role.id" trigger="click" @command="cmd => handleCommand(cmd, role)" @click.stop>
                <el-button link @click.stop><el-icon><MoreFilled /></el-icon></el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="duplicate">Duplicate Role</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </el-menu-item>
          </el-menu-item-group>
        </template>
        
        <template v-if="filteredCustomRoles.length > 0">
          <el-menu-item-group>
            <template #title>
              <el-text type="info" size="small" style="font-weight: 600; letter-spacing: 0.05em;">CUSTOM ROLES</el-text>
            </template>
            <el-menu-item 
              v-for="role in filteredCustomRoles" 
              :key="role.id" 
              :index="role.id"
            >
              <el-icon><User /></el-icon>
              <span style="flex: 1; overflow: hidden; text-overflow: ellipsis; padding-left: 4px;">{{ role.name }}</span>
              <el-dropdown v-if="selectedRoleId === role.id" trigger="click" @command="cmd => handleCommand(cmd, role)" @click.stop>
                <el-button link @click.stop><el-icon><MoreFilled /></el-icon></el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="edit">Edit Role</el-dropdown-item>
                    <el-dropdown-item command="duplicate">Duplicate</el-dropdown-item>
                    <el-dropdown-item divided command="delete" style="color: var(--el-color-danger);">Delete</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </el-menu-item>
          </el-menu-item-group>
        </template>

        <div v-if="!filteredSystemRoles.length && !filteredCustomRoles.length" style="padding: 32px 20px; text-align: center;">
          <el-text type="info">No roles found.</el-text>
        </div>
      </el-menu>
    </el-scrollbar>
    
    <div style="padding: 16px 20px; border-top: 1px solid var(--el-border-color-light);">
      <el-button type="primary" style="width: 100%;" @click="$emit('create-role')">
        <el-icon style="margin-right: 6px;"><Plus /></el-icon> Create Role
      </el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Search, Lock, User, MoreFilled, Plus } from '@element-plus/icons-vue'

const props = defineProps({
  roles: { type: Array, required: true },
  selectedRoleId: { type: String, default: '' }
})

const emit = defineEmits(['select-role', 'create-role', 'duplicate-role', 'edit-role', 'delete-role'])

const searchQuery = ref('')

const filteredRoles = computed(() => {
  if (!searchQuery.value) return props.roles
  const lowerQ = searchQuery.value.toLowerCase()
  return props.roles.filter(r => r.name.toLowerCase().includes(lowerQ))
})

const filteredSystemRoles = computed(() => filteredRoles.value.filter(r => r.isProtected))
const filteredCustomRoles = computed(() => filteredRoles.value.filter(r => !r.isProtected))

function handleCommand(cmd, role) {
  if (cmd === 'edit') emit('edit-role', role)
  if (cmd === 'duplicate') emit('duplicate-role', role)
  if (cmd === 'delete') emit('delete-role', role)
}
</script>

<style scoped>
::v-deep(.el-menu-item) {
  display: flex;
  align-items: center;
  height: 44px;
  line-height: 44px;
  margin-bottom: 4px;
  border-radius: 6px;
  margin: 0 12px 4px 12px;
}
::v-deep(.el-menu-item.is-active) {
  background-color: var(--el-color-primary-light-9);
  font-weight: 500;
}
</style>
