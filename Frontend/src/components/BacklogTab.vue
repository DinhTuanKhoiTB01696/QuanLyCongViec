<template>
  <!--
    Backlog tab — dựng theo ảnh Backlog1.jpeg / Backlog2.jpeg.
    Tái dùng issue side panel có sẵn (TaskDetailModal) qua emit @open-task ở component cha.
    [CẦN XÁC NHẬN] Quy tắc tách Board / Backlog: ảnh không chứng minh rõ. Tạm dùng
    semantics Jira: item thuộc 1 sprint => nhóm "Board"; item không thuộc sprint => "Backlog".
  -->
  <div class="backlog-tab">
    <!-- Toolbar -->
    <div class="backlog-toolbar">
      <div class="bt-left">
        <div class="search-wrap">
          <i class="fa-solid fa-magnifying-glass"></i>
          <input v-model="search" type="text" placeholder="Search backlog" />
        </div>
        <div class="avatar-stack">
          <i class="fa-regular fa-user stack-anon"></i>
          <span
            v-for="m in memberAvatars"
            :key="m.id"
            class="stack-avatar"
            :style="{ background: m.color }"
            :title="m.label"
          >{{ m.initials }}</span>
        </div>
        <button class="bt-btn"><i class="fa-solid fa-bars-staggered"></i> Filter</button>
      </div>
      <div class="bt-right">
        <button class="bt-icon" title="View settings"><i class="fa-solid fa-sliders"></i></button>
        <button class="bt-icon" title="More"><i class="fa-solid fa-ellipsis"></i></button>
      </div>
    </div>

    <!-- Groups -->
    <div class="backlog-body">
      <div v-for="group in groups" :key="group.key" class="backlog-group">
        <div class="group-head" @click="toggleGroup(group.key)">
          <input
            type="checkbox"
            class="grp-check"
            :checked="isGroupSelected(group)"
            @click.stop="toggleGroupSelection(group)"
          />
          <i class="grp-chevron fa-solid" :class="collapsed[group.key] ? 'fa-chevron-right' : 'fa-chevron-down'"></i>
          <span class="grp-name">{{ group.name }}</span>
          <span class="grp-count">({{ group.items.length }} work items)</span>
          <div class="grp-spacer"></div>
          <div class="grp-badges">
            <span class="cat-badge todo">{{ group.counts.todo }}</span>
            <span class="cat-badge progress">{{ group.counts.progress }}</span>
            <span class="cat-badge done">{{ group.counts.done }}</span>
          </div>
        </div>

        <div class="group-rows" v-show="!collapsed[group.key]">
          <div
            v-for="task in group.items"
            :key="task.id"
            class="bk-row"
            :class="{ selected: task.id === selectedTaskId }"
            @click="$emit('open-task', task)"
          >
            <input
              type="checkbox"
              class="row-check"
              :checked="selectedIds.has(task.id)"
              @click.stop="toggleRowSelection(task.id)"
            />
            <span class="row-key">{{ task.sequenceId || task.id.substring(0, 8).toUpperCase() }}</span>
            <span class="row-title">{{ task.title }}</span>

            <div class="row-right" @click.stop>
              <i v-if="hasSubtasks(task)" class="fa-solid fa-sitemap subtask-icon" title="Has subtasks"></i>

              <span v-if="task.dueDate" class="due-chip">
                <i class="fa-regular fa-calendar"></i> {{ formatDue(task.dueDate) }}
              </span>

              <el-dropdown trigger="click" @command="(val) => $emit('update-task', task, 'statusName', val, task.statusName)">
                <span class="status-chip" :class="statusCategory(task.statusName)">
                  {{ statusLabel(task.statusName) }} <i class="fa-solid fa-chevron-down"></i>
                </span>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item v-for="s in statusOptions" :key="s.name" :command="s.name">
                      <i :class="s.icon" :style="{ color: s.color }"></i> {{ s.label }}
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>

              <span class="row-avatar" :style="{ background: assigneeAvatar(task).color }" :title="assigneeAvatar(task).label">
                <template v-if="assigneeAvatar(task).initials">{{ assigneeAvatar(task).initials }}</template>
                <i v-else class="fa-regular fa-user"></i>
              </span>
            </div>
          </div>

          <div class="bk-create" @click="$emit('create-task', group.defaultStatus)">
            <i class="fa-solid fa-plus"></i> Create
          </div>
        </div>

        <div class="group-footer" v-if="!collapsed[group.key]">
          <span class="visible-note">{{ group.items.length }} of {{ group.items.length }} work items visible</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  projectMembers: { type: Array, default: () => [] },
  statusOptions: { type: Array, default: () => [] },
  selectedTaskId: { type: [String, Number], default: null }
})

defineEmits(['open-task', 'update-task', 'create-task'])

const search = ref('')
const collapsed = ref({})
const selectedIds = ref(new Set())

const AVATAR_COLORS = ['#22a06b', '#0c66e4', '#e2483d', '#e56910', '#6e5dc6', '#1d7afc', '#943d73']
const colorFor = (seed) => AVATAR_COLORS[(`${seed || ''}`.length) % AVATAR_COLORS.length]

const initialsOf = (label) => `${label || ''}`.trim().split(/\s+/).slice(0, 2).map(p => p[0]).join('').toUpperCase()

const memberAvatars = computed(() =>
  (props.projectMembers || []).slice(0, 4).map(m => {
    const label = m.fullName || m.name || m.email || 'User'
    return { id: m.userId || m.id, label, initials: initialsOf(label), color: colorFor(label) }
  })
)

const getAssigneeIds = (task) => Array.from(new Set([
  ...(Array.isArray(task.assigneeIds) ? task.assigneeIds : []),
  ...(Array.isArray(task.assignees) ? task.assignees.map(a => a.userId || a.id).filter(Boolean) : []),
  ...(task.assignedUserId ? [task.assignedUserId] : [])
]))

const assigneeAvatar = (task) => {
  const ids = getAssigneeIds(task)
  if (!ids.length) return { initials: '', label: 'Unassigned', color: 'transparent' }
  const member = (props.projectMembers || []).find(m => (m.userId || m.id) === ids[0])
  const label = member?.fullName || member?.name || member?.email || task.assigneeName || 'Assignee'
  return { initials: initialsOf(label), label, color: colorFor(label) }
}

const normalizeStatus = (v) => `${v || 'BACKLOG'}`.toUpperCase().replace(/\s+/g, ' ').trim()
const statusLabel = (v) => props.statusOptions.find(s => s.name === normalizeStatus(v))?.label || normalizeStatus(v)
const statusCategory = (v) => {
  const s = normalizeStatus(v)
  if (s.includes('PROGRESS')) return 'progress'
  if (s.includes('DONE') || s.includes('COMPLETE')) return 'done'
  return 'todo'
}

const hasSubtasks = (task) => Boolean(
  (Array.isArray(task.subtasks) && task.subtasks.length) ||
  task.hasChildren || task.subtaskCount > 0
)

const formatDue = (value) => {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

const matchesSearch = (task) => {
  const q = search.value.trim().toLowerCase()
  if (!q) return true
  return `${task.title || ''}`.toLowerCase().includes(q) ||
    `${task.sequenceId || ''}`.toLowerCase().includes(q)
}

const inSprint = (task) => Boolean(task.sprintId || task.sprintName)

const buildCounts = (items) => {
  const counts = { todo: 0, progress: 0, done: 0 }
  items.forEach(t => { counts[statusCategory(t.statusName)] += 1 })
  return counts
}

const groups = computed(() => {
  const filtered = (props.tasks || []).filter(matchesSearch)
  const board = filtered.filter(inSprint)
  const backlog = filtered.filter(t => !inSprint(t))
  return [
    { key: 'board', name: 'Board', items: board, counts: buildCounts(board), defaultStatus: 'TO DO' },
    { key: 'backlog', name: 'Backlog', items: backlog, counts: buildCounts(backlog), defaultStatus: 'BACKLOG' }
  ]
})

const toggleGroup = (key) => { collapsed.value[key] = !collapsed.value[key] }

const toggleRowSelection = (id) => {
  const next = new Set(selectedIds.value)
  next.has(id) ? next.delete(id) : next.add(id)
  selectedIds.value = next
}

const isGroupSelected = (group) => group.items.length > 0 && group.items.every(t => selectedIds.value.has(t.id))

const toggleGroupSelection = (group) => {
  const next = new Set(selectedIds.value)
  const allSelected = isGroupSelected(group)
  group.items.forEach(t => { allSelected ? next.delete(t.id) : next.add(t.id) })
  selectedIds.value = next
}
</script>

<style scoped>
.backlog-tab { display: flex; flex-direction: column; flex: 1; min-height: 0; overflow: hidden; background: var(--color-bg); }

.backlog-toolbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 12px 24px; flex-shrink: 0;
}
.bt-left { display: flex; align-items: center; gap: 12px; }
.bt-right { display: flex; align-items: center; gap: 6px; }
.search-wrap {
  display: flex; align-items: center; gap: 8px;
  border: 1px solid var(--color-border); border-radius: 4px;
  padding: 6px 10px; min-width: 220px; color: var(--color-text-muted);
}
.search-wrap input { background: transparent; border: none; outline: none; color: var(--color-text-primary); font-size: 13px; width: 100%; }
.avatar-stack { display: flex; align-items: center; }
.stack-anon { color: var(--color-text-muted); margin-right: 4px; }
.stack-avatar {
  width: 24px; height: 24px; border-radius: 50%; color: #fff;
  font-size: 10px; font-weight: 600; display: inline-flex; align-items: center; justify-content: center;
  margin-left: -6px; border: 2px solid var(--color-bg);
}
.bt-btn, .bt-icon {
  background: transparent; border: 1px solid transparent; color: var(--color-text-secondary);
  font-size: 13px; cursor: pointer; padding: 6px 10px; border-radius: 4px; display: flex; align-items: center; gap: 6px;
}
.bt-btn:hover, .bt-icon:hover { background: var(--color-border); }

.backlog-body { flex: 1; min-height: 0; overflow: auto; padding: 0 24px 24px; }
.backlog-group { margin-bottom: 24px; }

.group-head {
  display: flex; align-items: center; gap: 10px;
  padding: 8px 6px; cursor: pointer; border-bottom: 1px solid var(--color-border);
}
.grp-chevron { font-size: 11px; color: var(--color-text-muted); }
.grp-name { font-weight: 700; color: var(--color-text-primary); }
.grp-count { color: var(--color-text-muted); font-size: 13px; }
.grp-spacer { flex: 1; }
.grp-badges { display: flex; gap: 4px; }
.cat-badge {
  min-width: 22px; text-align: center; padding: 1px 6px; border-radius: 3px;
  font-size: 11px; font-weight: 600; background: var(--color-border); color: var(--color-text-muted);
}
.cat-badge.progress { background: #cce0ff; color: #0c66e4; }
.cat-badge.done { background: #dcfff1; color: #22a06b; }

.bk-row {
  display: flex; align-items: center; gap: 10px;
  padding: 8px 6px; border-bottom: 1px solid var(--color-border); cursor: pointer;
}
.bk-row:hover { background: var(--color-surface); }
.bk-row.selected { background: color-mix(in srgb, #0c66e4 12%, transparent); }
.row-key { color: var(--color-text-muted); font-size: 12px; min-width: 52px; }
.row-title { color: var(--color-text-primary); font-size: 13px; flex: 1; }
.row-right { display: flex; align-items: center; gap: 12px; }
.subtask-icon { color: var(--color-text-muted); font-size: 12px; }
.due-chip { display: inline-flex; align-items: center; gap: 4px; font-size: 12px; color: var(--color-text-secondary); }
.status-chip {
  display: inline-flex; align-items: center; gap: 6px; cursor: pointer;
  padding: 2px 8px; border-radius: 3px; font-size: 11px; font-weight: 600;
  background: var(--color-border); color: var(--color-text-secondary); text-transform: uppercase;
}
.status-chip.progress { background: #cce0ff; color: #0c66e4; }
.status-chip.done { background: #dcfff1; color: #22a06b; }
.status-chip i { font-size: 9px; }
.row-avatar {
  width: 24px; height: 24px; border-radius: 50%; color: #fff;
  font-size: 10px; font-weight: 600; display: inline-flex; align-items: center; justify-content: center;
}
.row-check, .grp-check { cursor: pointer; }

.bk-create {
  display: flex; align-items: center; gap: 8px; padding: 10px 6px;
  color: var(--color-text-muted); font-size: 13px; cursor: pointer;
}
.bk-create:hover { color: var(--color-text-primary); }
.group-footer { display: flex; justify-content: flex-end; padding: 6px; }
.visible-note { color: var(--color-text-muted); font-size: 12px; }
</style>
