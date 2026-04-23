<template>
  <NexusLayout>
    <div class="project-settings-page" v-loading="loading">
      <header class="settings-header">
        <div>
          <div class="breadcrumb">Project Settings</div>
          <h1>{{ project.name || 'Project settings' }}</h1>
          <p>Project-scoped administration for members, workflow, labels, modules, cycles, and project lifecycle.</p>
        </div>
        <div class="header-actions">
          <button class="secondary-btn" type="button" @click="goBack">Back to project</button>
          <button class="primary-btn" type="button" :disabled="savingGeneral" @click="saveGeneral">
            {{ savingGeneral ? 'Saving...' : 'Save general' }}
          </button>
        </div>
      </header>

      <div class="settings-shell">
        <aside class="settings-nav">
          <button
            v-for="tab in tabs"
            :key="tab.key"
            type="button"
            class="nav-tab"
            :class="{ active: activeTab === tab.key }"
            @click="activeTab = tab.key"
          >
            <span>{{ tab.label }}</span>
            <small>{{ tab.caption }}</small>
          </button>
        </aside>

        <section class="settings-content">
          <div v-if="activeTab === 'general'" class="settings-card">
            <div class="card-head">
              <div>
                <h2>General</h2>
                <p>Core project metadata and planning dates.</p>
              </div>
            </div>

            <div class="form-grid">
              <label>
                <span>Name</span>
                <input v-model="generalForm.name" type="text" placeholder="Project name" />
              </label>

              <label>
                <span>Key</span>
                <input :value="project.key || ''" type="text" disabled />
              </label>

              <label class="wide">
                <span>Description</span>
                <textarea v-model="generalForm.description" rows="4" placeholder="Describe the project"></textarea>
              </label>

              <label>
                <span>Start date</span>
                <input v-model="generalForm.startDate" type="date" />
              </label>

              <label>
                <span>End date</span>
                <input v-model="generalForm.endDate" type="date" />
              </label>
            </div>

            <div class="meta-strip">
              <span>Visibility: {{ project.networkType || 'Public' }}</span>
              <span>Lead: {{ project.leadName || 'Not assigned' }}</span>
              <span>Members: {{ members.length }}</span>
              <span>Archived: {{ project.isArchived ? 'Yes' : 'No' }}</span>
            </div>
          </div>

          <div v-else-if="activeTab === 'members'" class="settings-card">
            <div class="card-head">
              <div>
                <h2>Members & Roles</h2>
                <p>Invite members, adjust roles, and remove access without using system admin.</p>
              </div>
            </div>

            <div class="inline-form invite-grid">
              <label>
                <span>Email</span>
                <input v-model="inviteForm.email" type="email" placeholder="member@company.com" />
              </label>
              <label>
                <span>Role</span>
                <select v-model="inviteForm.role">
                  <option v-for="role in projectRoleOptions" :key="role" :value="role">{{ role }}</option>
                </select>
              </label>
              <button class="secondary-btn" type="button" :disabled="savingInvite" @click="inviteMember">
                {{ savingInvite ? 'Inviting...' : 'Invite member' }}
              </button>
            </div>

            <div v-if="members.length === 0" class="empty-state">No project members found.</div>
            <div v-else class="stack-list">
              <div v-for="member in members" :key="member.userId" class="stack-row">
                <div class="row-main">
                  <strong>{{ member.fullName || member.email }}</strong>
                  <p>{{ member.email }}</p>
                </div>
                <div class="row-actions">
                  <select :value="member.projectRole" @change="updateMemberRole(member, $event.target.value)">
                    <option v-for="role in projectRoleOptions" :key="`${member.userId}-${role}`" :value="role">{{ role }}</option>
                  </select>
                  <button class="danger-outline-btn" type="button" @click="removeMember(member)">Remove</button>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="activeTab === 'states'" class="settings-card">
            <div class="card-head">
              <div>
                <h2>States</h2>
                <p>Project-specific workflow states used by work items.</p>
              </div>
            </div>

            <div class="inline-form state-grid">
              <label>
                <span>Name</span>
                <input v-model="newState.name" type="text" placeholder="Blocked" />
              </label>
              <label>
                <span>Color</span>
                <input v-model="newState.colorCode" type="color" />
              </label>
              <label>
                <span>Position</span>
                <input v-model.number="newState.position" type="number" min="0" />
              </label>
              <button class="secondary-btn" type="button" :disabled="savingState" @click="createState">
                {{ savingState ? 'Adding...' : 'Add state' }}
              </button>
            </div>

            <div class="stack-list">
              <div v-for="status in taskStatuses" :key="status.id" class="stack-row">
                <div class="row-main state-row">
                  <input v-model="status.name" type="text" />
                  <input v-model="status.colorCode" type="color" />
                  <input v-model.number="status.position" type="number" min="0" />
                </div>
                <div class="row-actions">
                  <button class="secondary-btn" type="button" @click="saveState(status)">Save</button>
                  <button class="danger-outline-btn" type="button" @click="deleteState(status)">Delete</button>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="activeTab === 'labels'" class="settings-card">
            <div class="card-head">
              <div>
                <h2>Labels</h2>
                <p>Reusable project labels for classification and planning.</p>
              </div>
            </div>

            <div class="inline-form label-grid">
              <label>
                <span>Name</span>
                <input v-model="newLabel.name" type="text" placeholder="Customer" />
              </label>
              <label>
                <span>Color</span>
                <input v-model="newLabel.colorCode" type="color" />
              </label>
              <label class="wide">
                <span>Description</span>
                <input v-model="newLabel.description" type="text" placeholder="Optional description" />
              </label>
              <button class="secondary-btn" type="button" :disabled="savingLabel" @click="createLabel">
                {{ savingLabel ? 'Adding...' : 'Add label' }}
              </button>
            </div>

            <div v-if="labels.length === 0" class="empty-state">No labels configured yet.</div>
            <div v-else class="stack-list">
              <div v-for="label in labels" :key="label.id" class="stack-row">
                <div class="row-main editable-grid compact-grid">
                  <label>
                    <span>Name</span>
                    <input v-model="label.name" type="text" />
                  </label>
                  <label>
                    <span>Color</span>
                    <input v-model="label.colorCode" type="color" />
                  </label>
                  <label class="wide">
                    <span>Description</span>
                    <input v-model="label.description" type="text" placeholder="Optional description" />
                  </label>
                </div>
                <div class="row-actions">
                  <button class="secondary-btn" type="button" @click="saveLabel(label)">Save</button>
                  <button class="danger-outline-btn" type="button" @click="deleteLabel(label)">Delete</button>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="activeTab === 'modules'" class="settings-card">
            <div class="card-head">
              <div>
                <h2>Modules</h2>
                <p>Manage project modules and their ownership.</p>
              </div>
              <button class="secondary-btn" type="button" @click="goToModulesWorkspace">Open modules workspace</button>
            </div>

            <div class="inline-form module-grid">
              <label>
                <span>Name</span>
                <input v-model="newModule.name" type="text" placeholder="Platform" />
              </label>
              <label>
                <span>Status</span>
                <select v-model="newModule.status">
                  <option value="Backlog">Backlog</option>
                  <option value="Planned">Planned</option>
                  <option value="In Progress">In Progress</option>
                  <option value="Paused">Paused</option>
                  <option value="Completed">Completed</option>
                </select>
              </label>
              <label>
                <span>Lead</span>
                <select v-model="newModule.leadId">
                  <option :value="null">No lead</option>
                  <option v-for="member in members" :key="member.userId" :value="member.userId">
                    {{ member.fullName || member.email }}
                  </option>
                </select>
              </label>
              <label>
                <span>Start date</span>
                <input v-model="newModule.startDate" type="date" :min="todayDate" />
              </label>
              <label>
                <span>Target date</span>
                <input v-model="newModule.targetDate" type="date" :min="newModule.startDate || todayDate" />
              </label>
              <label class="wide">
                <span>Description</span>
                <input v-model="newModule.description" type="text" placeholder="Optional description" />
              </label>
              <button class="secondary-btn" type="button" :disabled="savingModule" @click="createModule">
                {{ savingModule ? 'Adding...' : 'Add module' }}
              </button>
            </div>

              <div class="helper-panel">
                <div class="meta-strip compact">
                  <span>Active modules: {{ activeModules.length }}</span>
                  <span>Disabled modules: {{ disabledModules.length }}</span>
                  <span>Disabled modules are shown in the restore section below.</span>
                </div>
              </div>

              <div class="helper-panel">
                <div class="section-split">
                  <h3>Restore modules</h3>
                  <p v-if="disabledModules.length">Disabled modules appear here first so you can restore them quickly.</p>
                  <p v-else>No disabled modules yet. After you disable one, it will appear here.</p>
                </div>
                <div v-if="disabledModules.length" class="stack-list">
                  <div v-for="module in disabledModules" :key="`restore-top-${module.id}`" class="stack-row">
                    <div class="row-main">
                      <strong>{{ module.name }}</strong>
                      <p>{{ module.description || 'Disabled module ready to restore.' }}</p>
                    </div>
                    <div class="row-actions">
                      <button class="secondary-btn" type="button" @click="restoreModule(module)">Restore</button>
                    </div>
                  </div>
                </div>
              </div>

              <div v-if="modules.length === 0" class="empty-state">No modules configured yet.</div>
              <template v-else>
                <div class="section-split">
                  <h3>Active modules</h3>
                  <p>Save updates normally. Disabled modules move to the restore list below.</p>
                </div>
                <div class="stack-list">
                  <div v-for="module in activeModules" :key="module.id" class="stack-row">
                    <div class="row-main editable-grid">
                      <label>
                        <span>Name</span>
                        <input v-model="module.name" type="text" />
                      </label>
                      <label>
                        <span>Status</span>
                        <select v-model="module.status">
                          <option value="Backlog">Backlog</option>
                          <option value="Planned">Planned</option>
                          <option value="In Progress">In Progress</option>
                          <option value="Paused">Paused</option>
                          <option value="Completed">Completed</option>
                          <option value="Disabled">Disabled</option>
                        </select>
                      </label>
                      <label>
                        <span>Lead</span>
                        <select v-model="module.leadId">
                          <option :value="null">No lead</option>
                          <option v-for="member in members" :key="`${module.id}-${member.userId}`" :value="member.userId">
                            {{ member.fullName || member.email }}
                          </option>
                        </select>
                      </label>
                      <label>
                        <span>Start date</span>
                        <input v-model="module.startDate" type="date" :min="todayDate" />
                      </label>
                      <label>
                        <span>Target date</span>
                        <input v-model="module.targetDate" type="date" :min="module.startDate || todayDate" />
                      </label>
                      <label class="wide">
                        <span>Description</span>
                        <input v-model="module.description" type="text" placeholder="Optional description" />
                      </label>
                      <div class="meta-strip compact">
                        <span>Lead: {{ module.leadName || 'Unassigned' }}</span>
                        <span>Work items: {{ module.issueCount || 0 }}</span>
                        <span>Progress: {{ module.progressPercent || 0 }}%</span>
                      </div>
                    </div>
                    <div class="row-actions">
                      <button class="secondary-btn" type="button" @click="saveModule(module)">Save</button>
                      <button class="danger-outline-btn" type="button" @click="deleteModule(module)">Disable</button>
                    </div>
                  </div>
                </div>

                <div class="section-split">
                  <h3>Disabled modules</h3>
                  <p>Restore here when you want the module to appear again in project workspaces.</p>
                </div>
                <div v-if="disabledModules.length === 0" class="empty-state">No disabled modules.</div>
                <div v-else class="stack-list">
                  <div v-for="module in disabledModules" :key="`disabled-${module.id}`" class="stack-row">
                    <div class="row-main editable-grid">
                      <label>
                        <span>Name</span>
                        <input v-model="module.name" type="text" />
                      </label>
                      <label>
                        <span>Status</span>
                        <select v-model="module.status">
                          <option value="Disabled">Disabled</option>
                          <option value="Backlog">Backlog</option>
                          <option value="Planned">Planned</option>
                          <option value="In Progress">In Progress</option>
                          <option value="Paused">Paused</option>
                          <option value="Completed">Completed</option>
                        </select>
                      </label>
                      <label>
                        <span>Lead</span>
                        <select v-model="module.leadId">
                          <option :value="null">No lead</option>
                          <option v-for="member in members" :key="`${module.id}-restore-${member.userId}`" :value="member.userId">
                            {{ member.fullName || member.email }}
                          </option>
                        </select>
                      </label>
                      <label>
                        <span>Start date</span>
                        <input v-model="module.startDate" type="date" :min="todayDate" />
                      </label>
                      <label>
                        <span>Target date</span>
                        <input v-model="module.targetDate" type="date" :min="module.startDate || todayDate" />
                      </label>
                      <label class="wide">
                        <span>Description</span>
                        <input v-model="module.description" type="text" placeholder="Optional description" />
                      </label>
                      <div class="meta-strip compact">
                        <span>Lead: {{ module.leadName || 'Unassigned' }}</span>
                        <span>Work items: {{ module.issueCount || 0 }}</span>
                        <span>Progress: {{ module.progressPercent || 0 }}%</span>
                      </div>
                    </div>
                    <div class="row-actions">
                      <button class="secondary-btn" type="button" @click="saveModule(module)">Save</button>
                      <button class="secondary-btn" type="button" @click="restoreModule(module)">Restore</button>
                    </div>
                  </div>
                </div>
              </template>
            </div>

          <div v-else-if="activeTab === 'cycles'" class="settings-card">
            <div class="card-head">
              <div>
                <h2>Cycles</h2>
                <p>Plan cycles from project settings and jump into carry-over planning.</p>
              </div>
              <button class="secondary-btn" type="button" @click="goToCyclesWorkspace">Open cycles workspace</button>
            </div>

            <div class="inline-form cycle-grid">
              <label>
                <span>Name</span>
                <input v-model="newCycle.name" type="text" placeholder="Cycle 12" />
              </label>
              <label>
                <span>Start date</span>
                <input v-model="newCycle.startDate" type="date" :min="todayDate" />
              </label>
              <label>
                <span>End date</span>
                <input v-model="newCycle.endDate" type="date" :min="newCycle.startDate || todayDate" />
              </label>
              <label class="wide">
                <span>Description</span>
                <input v-model="newCycle.description" type="text" placeholder="Optional description" />
              </label>
              <button class="secondary-btn" type="button" :disabled="savingCycle" @click="createCycle">
                {{ savingCycle ? 'Adding...' : 'Add cycle' }}
              </button>
            </div>

            <div v-if="sprints.length === 0" class="empty-state">No cycles configured yet.</div>
            <div v-else class="stack-list">
              <div v-for="sprint in sprints" :key="sprint.id" class="stack-row">
                <div class="row-main editable-grid compact-grid">
                  <label>
                    <span>Name</span>
                    <input v-model="sprint.name" type="text" />
                  </label>
                  <label>
                    <span>Start date</span>
                    <input v-model="sprint.startDate" type="date" :min="todayDate" />
                  </label>
                  <label>
                    <span>End date</span>
                    <input v-model="sprint.endDate" type="date" :min="sprint.startDate || todayDate" />
                  </label>
                  <div class="meta-strip compact">
                    <span>State: {{ sprint.state }}</span>
                    <span>Favorite: {{ sprint.isFavorite ? 'Yes' : 'No' }}</span>
                    <span>Progress: {{ sprint.progressPercent || 0 }}%</span>
                    <span>Tasks: {{ sprint.taskCount || 0 }}</span>
                  </div>
                </div>
                <div class="row-actions">
                  <button class="secondary-btn" type="button" @click="saveCycle(sprint)">Save</button>
                  <button class="secondary-btn" type="button" @click="toggleFavoriteCycle(sprint)">
                    {{ sprint.isFavorite ? 'Unfavorite' : 'Favorite' }}
                  </button>
                  <button v-if="sprint.state !== 'Active'" class="secondary-btn" type="button" @click="startCycle(sprint)">Start</button>
                  <button v-if="sprint.state === 'Active'" class="secondary-btn" type="button" @click="closeCycleToBacklog(sprint)">Close to backlog</button>
                  <button class="secondary-btn" type="button" @click="openCycleCarryOver(sprint)">Carry-over</button>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="activeTab === 'integrations'" class="settings-card">
            <div class="card-head">
              <div>
                <h2>Integrations</h2>
                <p>Configure provider connections at project scope without opening system admin settings.</p>
              </div>
            </div>

            <div v-if="integrations.length === 0" class="empty-state">
              GitHub integration is not configured for this project yet.
            </div>
            <div v-else class="stack-list">
              <div v-for="integration in integrations" :key="integration.provider" class="stack-row">
                <div class="row-main editable-grid integration-grid">
                  <label>
                    <span>Provider</span>
                    <input :value="integration.displayName" type="text" disabled />
                  </label>
                  <label>
                    <span>Enabled</span>
                    <select v-model="integration.enabled">
                      <option :value="true">Enabled</option>
                      <option :value="false">Disabled</option>
                    </select>
                  </label>
                  <label>
                    <span>Repository</span>
                    <input v-model="integration.projectKey" type="text" placeholder="owner/repository" />
                  </label>
                  <label class="wide">
                    <span>API endpoint</span>
                    <input v-model="integration.endpoint" type="text" :placeholder="httpsPlaceholders[integration.provider] || 'https://api.github.com/repos/owner/repository'" />
                  </label>
                  <label>
                    <span>Access token</span>
                    <input v-model="integration.secret" type="password" placeholder="Stored only for this project" />
                  </label>
                  <label class="wide">
                    <span>Notes</span>
                    <input v-model="integration.notes" type="text" placeholder="Used by AI repo analysis and task breakdown" />
                  </label>
                  <div class="meta-strip compact">
                    <span>Provider: {{ integration.provider }}</span>
                    <span>Updated: {{ formatDateLabel(integration.updatedAt) }}</span>
                  </div>
                </div>
                <div class="row-actions">
                  <button class="secondary-btn" type="button" @click="resetIntegration(integration.provider)">Reset</button>
                </div>
              </div>
            </div>

            <div class="danger-actions integration-actions">
              <button class="secondary-btn" type="button" :disabled="savingIntegrations" @click="loadIntegrations">
                Reload integrations
              </button>
              <button class="primary-btn" type="button" :disabled="savingIntegrations" @click="saveIntegrations">
                {{ savingIntegrations ? 'Saving...' : 'Save integrations' }}
              </button>
            </div>

            <div class="empty-state helper-panel">
              Only GitHub is available here so AI can analyze the connected repository and break work into tasks later.
            </div>
          </div>

          <div v-else class="settings-card danger-card">
            <div class="card-head">
              <div>
                <h2>Danger Zone</h2>
                <p>Archive, restore, or delete this project.</p>
              </div>
            </div>

            <div class="danger-actions">
              <button class="danger-outline-btn" type="button" @click="toggleArchive">
                {{ project.isArchived ? 'Restore project' : 'Archive project' }}
              </button>
              <button class="danger-btn" type="button" @click="deleteProject">Delete project</button>
            </div>
          </div>
        </section>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import axiosClient from '@/api/axiosClient'
import { broadcastAdminRealtime, subscribeAdminRealtime } from '@/utils/adminRealtime'

const route = useRoute()
const router = useRouter()
const projectId = route.params.id

const tabs = [
  { key: 'general', label: 'General', caption: 'metadata' },
  { key: 'members', label: 'Members & Roles', caption: 'access' },
  { key: 'states', label: 'States', caption: 'workflow' },
  { key: 'labels', label: 'Labels', caption: 'classification' },
  { key: 'modules', label: 'Modules', caption: 'planning' },
  { key: 'cycles', label: 'Cycles', caption: 'iteration planning' },
  { key: 'integrations', label: 'Integrations', caption: 'project providers' },
  { key: 'danger', label: 'Danger Zone', caption: 'destructive actions' }
]

const projectRoleOptions = ['PROJECT_MANAGER', 'PROJECT_LEAD', 'PM', 'PO', 'Developer', 'Member', 'Guest', 'Stakeholder']

const activeTab = ref('general')
const loading = ref(false)
const savingGeneral = ref(false)
const savingInvite = ref(false)
const savingState = ref(false)
const savingLabel = ref(false)
const savingModule = ref(false)
const savingCycle = ref(false)
const savingIntegrations = ref(false)

const project = ref({})
const members = ref([])
const taskStatuses = ref([])
const labels = ref([])
const modules = ref([])
const sprints = ref([])
const integrations = ref([])
const activeModules = computed(() => modules.value.filter(module => module.status !== 'Disabled'))
const disabledModules = computed(() => modules.value.filter(module => module.status === 'Disabled'))

const generalForm = ref({
  name: '',
  description: '',
  startDate: '',
  endDate: ''
})

const inviteForm = ref({
  email: '',
  role: 'Developer'
})

const newState = ref({
  name: '',
  colorCode: '#3b82f6',
  position: 0
})

const newLabel = ref({
  name: '',
  colorCode: '#3b82f6',
  description: ''
})

const newModule = ref({
  name: '',
  description: '',
  status: 'Backlog',
  leadId: null,
  startDate: '',
  targetDate: ''
})

const newCycle = ref({
  name: '',
  description: '',
  startDate: '',
  endDate: ''
})

const httpsPlaceholders = {
  github: 'https://api.github.com/repos/org/repo'
}

const todayDate = new Date().toISOString().slice(0, 10)

const normalizeDateInput = (value) => {
  if (!value) return ''
  const raw = String(value)
  return raw.includes('T') ? raw.slice(0, 10) : raw.slice(0, 10)
}

const toLocalDateTime = (value) => (value ? `${value}T00:00:00` : null)

const formatDateLabel = (value) => {
  if (!value) return 'No date'
  const raw = String(value).split('T')[0]
  const [year, month, day] = raw.split('-').map(Number)
  if (!year || !month || !day) return raw
  return new Date(year, month - 1, day).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}

const normalizeIntegration = (integration) => ({
  provider: integration.provider,
  displayName: integration.displayName || integration.provider,
  enabled: Boolean(integration.enabled),
  endpoint: integration.endpoint || '',
  projectKey: integration.projectKey || '',
  secret: integration.secret || '',
  notes: integration.notes || '',
  updatedAt: integration.updatedAt || ''
})

const isPastDate = (value) => {
  if (!value) return false
  return normalizeDateInput(value) < todayDate
}

const normalizeModuleStatus = (status) => {
  const value = `${status || ''}`.trim().toLowerCase()
  if (!value) return 'Backlog'
  if (value === 'disabled') return 'Disabled'
  if (value === 'planned') return 'Planned'
  if (value === 'in progress' || value === 'inprogress') return 'In Progress'
  if (value === 'paused') return 'Paused'
  if (value === 'completed' || value === 'complete') return 'Completed'
  return 'Backlog'
}

const notifyProjectSettingsRealtime = (type = 'project-settings-updated') => {
  broadcastAdminRealtime(type, { projectId })
}

const loadProjectSettings = async () => {
  loading.value = true
  try {
    const [settingsRes, labelsRes, modulesRes, cyclesRes, statusesRes, integrationsRes] = await Promise.all([
      axiosClient.get(`/projects/${projectId}/settings`),
      axiosClient.get(`/projects/${projectId}/labels`),
      axiosClient.get(`/projects/${projectId}/modules`, { params: { page: 1, pageSize: 50, includeDisabled: true } }),
      axiosClient.get(`/projects/${projectId}/sprints`),
      axiosClient.get(`/projects/${projectId}/task-statuses`),
      axiosClient.get(`/projects/${projectId}/integrations`)
    ])

    const settings = settingsRes.data?.data || {}
    project.value = settings.project || {}
    members.value = settings.members || []
    labels.value = (labelsRes.data?.data || []).map(label => ({
      ...label,
      colorCode: label.colorCode || label.color || '#3b82f6'
    }))
    modules.value = (modulesRes.data?.data || []).map(module => ({
      ...module,
      status: normalizeModuleStatus(module.status),
      startDate: normalizeDateInput(module.startDate),
      targetDate: normalizeDateInput(module.targetDate),
      leadId: module.leadId || null
    }))
    sprints.value = (cyclesRes.data?.data || []).map(sprint => ({
      ...sprint,
      startDate: normalizeDateInput(sprint.startDate),
      endDate: normalizeDateInput(sprint.endDate)
    }))
    taskStatuses.value = (statusesRes.data?.data || []).map((status, index) => ({
      ...status,
      colorCode: status.colorCode || '#3b82f6',
      position: status.position ?? index
    }))
    newState.value.position = taskStatuses.value.length
    integrations.value = (integrationsRes.data?.data || [])
      .map(normalizeIntegration)
      .filter(integration => integration.provider === 'github')

    generalForm.value = {
      name: project.value.name || '',
      description: project.value.description || '',
      startDate: normalizeDateInput(project.value.startDate),
      endDate: normalizeDateInput(project.value.endDate)
    }
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not load project settings')
    router.replace(`/space/${projectId}`)
  } finally {
    loading.value = false
  }
}

const loadIntegrations = async () => {
  try {
    const response = await axiosClient.get(`/projects/${projectId}/integrations`)
    integrations.value = (response.data?.data || [])
      .map(normalizeIntegration)
      .filter(integration => integration.provider === 'github')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not load project integrations')
  }
}

const saveGeneral = async () => {
  if (!generalForm.value.name.trim() || !generalForm.value.startDate) {
    ElMessage.warning('Project name and start date are required')
    return
  }

  savingGeneral.value = true
  try {
    const response = await axiosClient.put(`/projects/${projectId}`, {
      name: generalForm.value.name.trim(),
      description: generalForm.value.description?.trim() || '',
      startDate: generalForm.value.startDate || null,
      endDate: generalForm.value.endDate || null,
      departmentId: project.value.departmentId || null
    })
    const updatedProject = response.data?.data || {}
    project.value = {
      ...project.value,
      ...updatedProject,
      name: updatedProject.name || generalForm.value.name.trim(),
      description: updatedProject.description ?? generalForm.value.description?.trim() ?? '',
      startDate: updatedProject.startDate || generalForm.value.startDate || null,
      endDate: updatedProject.endDate || generalForm.value.endDate || null
    }
    ElMessage.success('Project general settings updated')
    notifyProjectSettingsRealtime()
    await loadProjectSettings()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not save project settings')
  } finally {
    savingGeneral.value = false
  }
}

const inviteMember = async () => {
  if (!inviteForm.value.email.trim()) {
    ElMessage.warning('Member email is required')
    return
  }

  savingInvite.value = true
  try {
    await axiosClient.post(`/projects/${projectId}/members`, {
      email: inviteForm.value.email.trim(),
      role: inviteForm.value.role
    })
    inviteForm.value.email = ''
    inviteForm.value.role = 'Developer'
    ElMessage.success('Member invited successfully')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not invite member')
  } finally {
    savingInvite.value = false
  }
}

const updateMemberRole = async (member, role) => {
  try {
    await axiosClient.put(`/projects/${projectId}/members/${member.userId}/role`, { role })
    member.projectRole = role
    ElMessage.success('Member role updated')
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not update member role')
    await loadProjectSettings()
  }
}

const removeMember = async (member) => {
  try {
    await ElMessageBox.confirm(`Remove ${member.fullName || member.email} from this project?`, 'Remove member', { type: 'warning' })
    await axiosClient.delete(`/projects/${projectId}/members/${member.userId}`)
    ElMessage.success('Member removed')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || 'Could not remove member')
    }
  }
}

const createState = async () => {
  if (!newState.value.name.trim()) {
    ElMessage.warning('State name is required')
    return
  }

  savingState.value = true
  try {
    await axiosClient.post(`/projects/${projectId}/task-statuses`, {
      name: newState.value.name.trim(),
      colorCode: newState.value.colorCode,
      position: newState.value.position
    })
    newState.value = {
      name: '',
      colorCode: '#3b82f6',
      position: taskStatuses.value.length + 1
    }
    ElMessage.success('State created')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create state')
  } finally {
    savingState.value = false
  }
}

const saveState = async (status) => {
  try {
    await axiosClient.put(`/projects/${projectId}/task-statuses/${status.id}`, {
      name: status.name,
      colorCode: status.colorCode,
      position: status.position
    })
    ElMessage.success('State updated')
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not update state')
    await loadProjectSettings()
  }
}

const deleteState = async (status) => {
  try {
    await ElMessageBox.confirm(`Delete state "${status.name}"?`, 'Delete state', { type: 'warning' })
    await axiosClient.delete(`/projects/${projectId}/task-statuses/${status.id}`)
    ElMessage.success('State deleted')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || 'Could not delete state')
    }
  }
}

const createLabel = async () => {
  if (!newLabel.value.name.trim()) {
    ElMessage.warning('Label name is required')
    return
  }

  savingLabel.value = true
  try {
    await axiosClient.post(`/projects/${projectId}/labels`, {
      name: newLabel.value.name.trim(),
      colorCode: newLabel.value.colorCode,
      description: newLabel.value.description?.trim() || ''
    })
    newLabel.value = { name: '', colorCode: '#3b82f6', description: '' }
    ElMessage.success('Label created')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create label')
  } finally {
    savingLabel.value = false
  }
}

const deleteLabel = async (label) => {
  try {
    await ElMessageBox.confirm(`Delete label "${label.name}"?`, 'Delete label', { type: 'warning' })
    await axiosClient.delete(`/projects/${projectId}/labels/${label.id}`)
    ElMessage.success('Label deleted')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || 'Could not delete label')
    }
  }
}

const saveLabel = async (label) => {
  try {
    await axiosClient.put(`/projects/${projectId}/labels/${label.id}`, {
      name: label.name?.trim(),
      colorCode: label.colorCode,
      description: label.description?.trim() || ''
    })
    ElMessage.success('Label updated')
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not update label')
    await loadProjectSettings()
  }
}

const createModule = async () => {
  if (!newModule.value.name.trim()) {
    ElMessage.warning('Module name is required')
    return
  }
  if (isPastDate(newModule.value.startDate) || isPastDate(newModule.value.targetDate)) {
    ElMessage.warning('Past dates are not allowed for modules')
    return
  }
  if (newModule.value.startDate && newModule.value.targetDate && newModule.value.targetDate < newModule.value.startDate) {
    ElMessage.warning('Target date must be on or after the start date')
    return
  }

  savingModule.value = true
  try {
    await axiosClient.post(`/projects/${projectId}/modules`, {
      name: newModule.value.name.trim(),
      description: newModule.value.description?.trim() || '',
      status: newModule.value.status,
      leadId: newModule.value.leadId,
      startDate: toLocalDateTime(newModule.value.startDate),
      targetDate: toLocalDateTime(newModule.value.targetDate)
    })
    newModule.value = {
      name: '',
      description: '',
      status: 'Backlog',
      leadId: null,
      startDate: '',
      targetDate: ''
    }
    ElMessage.success('Module created')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create module')
  } finally {
    savingModule.value = false
  }
}

const deleteModule = async (module) => {
  try {
    await ElMessageBox.confirm(`Disable module "${module.name}"?`, 'Disable module', { type: 'warning' })
    await axiosClient.delete(`/projects/${projectId}/modules/${module.id}`)
    ElMessage.success('Module disabled')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || 'Could not disable module')
    }
  }
}

const saveModule = async (module) => {
  if (isPastDate(module.startDate) || isPastDate(module.targetDate)) {
    ElMessage.warning('Past dates are not allowed for modules')
    return
  }
  if (module.startDate && module.targetDate && module.targetDate < module.startDate) {
    ElMessage.warning('Target date must be on or after the start date')
    return
  }
  try {
    await axiosClient.put(`/projects/${projectId}/modules/${module.id}`, {
      name: module.name?.trim(),
      description: module.description?.trim() || '',
      status: module.status,
      leadId: module.leadId,
      startDate: toLocalDateTime(module.startDate),
      targetDate: toLocalDateTime(module.targetDate)
    })
    ElMessage.success('Module updated')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not update module')
  }
}

const restoreModule = async (module) => {
  try {
    await axiosClient.put(`/projects/${projectId}/modules/${module.id}`, {
      status: 'Backlog'
    })
    ElMessage.success('Module restored')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not restore module')
  }
}

const createCycle = async () => {
  if (!newCycle.value.name.trim() || !newCycle.value.startDate || !newCycle.value.endDate) {
    ElMessage.warning('Cycle name, start date, and end date are required')
    return
  }
  if (isPastDate(newCycle.value.startDate) || isPastDate(newCycle.value.endDate)) {
    ElMessage.warning('Past dates are not allowed for cycles')
    return
  }
  if (newCycle.value.endDate < newCycle.value.startDate) {
    ElMessage.warning('End date must be on or after the start date')
    return
  }

  savingCycle.value = true
  try {
    await axiosClient.post(`/projects/${projectId}/sprints`, {
      name: newCycle.value.name.trim(),
      description: newCycle.value.description?.trim() || '',
      startDate: toLocalDateTime(newCycle.value.startDate),
      endDate: toLocalDateTime(newCycle.value.endDate)
    })
    newCycle.value = { name: '', description: '', startDate: '', endDate: '' }
    ElMessage.success('Cycle created')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create cycle')
  } finally {
    savingCycle.value = false
  }
}

const saveCycle = async (sprint) => {
  if (isPastDate(sprint.startDate) || isPastDate(sprint.endDate)) {
    ElMessage.warning('Past dates are not allowed for cycles')
    return
  }
  if (sprint.startDate && sprint.endDate && sprint.endDate < sprint.startDate) {
    ElMessage.warning('End date must be on or after the start date')
    return
  }
  try {
    await axiosClient.put(`/projects/${projectId}/sprints/${sprint.id}`, {
      name: sprint.name?.trim(),
      startDate: toLocalDateTime(sprint.startDate),
      endDate: toLocalDateTime(sprint.endDate)
    })
    ElMessage.success('Cycle updated')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not update cycle')
  }
}

const startCycle = async (sprint) => {
  try {
    await axiosClient.post(`/projects/${projectId}/sprints/${sprint.id}/start`)
    ElMessage.success('Cycle started')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not start cycle')
  }
}

const closeCycleToBacklog = async (sprint) => {
  try {
    await ElMessageBox.confirm(
      `Close "${sprint.name}" and move unfinished work items to backlog?`,
      'Close cycle',
      { type: 'warning' }
    )
    await axiosClient.post(`/projects/${projectId}/sprints/${sprint.id}/close`, { targetSprintId: null })
    ElMessage.success('Cycle closed and unfinished work moved to backlog')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || 'Could not close cycle')
    }
  }
}

const toggleFavoriteCycle = async (sprint) => {
  try {
    await axiosClient.patch(`/projects/${projectId}/sprints/${sprint.id}/favorite`)
    sprint.isFavorite = !sprint.isFavorite
    ElMessage.success(sprint.isFavorite ? 'Cycle marked as favorite' : 'Cycle removed from favorites')
    notifyProjectSettingsRealtime('project-settings-favorite-updated')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not update cycle favorite state')
    await loadProjectSettings()
  }
}

const openCycleCarryOver = (sprint) => {
  router.push({
    name: 'CyclesView',
    params: { id: projectId },
    query: { carryOverFromSprintId: sprint.id }
  })
}

const saveIntegrations = async () => {
  savingIntegrations.value = true
  try {
    await axiosClient.put(`/projects/${projectId}/integrations`, {
      items: integrations.value
        .filter(integration => integration.provider === 'github')
        .map(integration => ({
        provider: integration.provider,
        displayName: integration.displayName,
        enabled: integration.enabled,
        endpoint: integration.endpoint?.trim() || null,
        projectKey: integration.projectKey?.trim() || null,
        secret: integration.secret?.trim() || null,
        notes: integration.notes?.trim() || null,
        updatedAt: integration.updatedAt || null
      }))
    })
    ElMessage.success('Project integrations updated')
    await loadIntegrations()
    notifyProjectSettingsRealtime('project-settings-integrations-updated')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not update project integrations')
  } finally {
    savingIntegrations.value = false
  }
}

const resetIntegration = async (provider) => {
  try {
    const current = integrations.value.find(item => item.provider === provider)
    if (!current) return

    const response = await axiosClient.get(`/projects/${projectId}/integrations`)
    const freshItems = (response.data?.data || []).map(normalizeIntegration)
    const next = freshItems.find(item => item.provider === provider)
    if (!next) return

    Object.assign(current, next)
    ElMessage.success(`${next.displayName} reset`)
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not reset integration')
  }
}

const toggleArchive = async () => {
  const action = project.value.isArchived ? 'restore' : 'archive'
  try {
    await ElMessageBox.confirm(
      action === 'archive'
        ? 'Archive this project and remove it from active project lists?'
        : 'Restore this project back to active lists?',
      action === 'archive' ? 'Archive project' : 'Restore project',
      { type: 'warning' }
    )

    await axiosClient.put(`/projects/${projectId}/${action}`)
    ElMessage.success(action === 'archive' ? 'Project archived' : 'Project restored')
    await loadProjectSettings()
    notifyProjectSettingsRealtime()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || 'Could not update project status')
    }
  }
}

const deleteProject = async () => {
  try {
    await ElMessageBox.confirm(
      'Delete this project? This action cannot be undone from the project settings screen.',
      'Delete project',
      { type: 'warning', confirmButtonText: 'Delete' }
    )

    await axiosClient.delete(`/projects/${projectId}`)
    ElMessage.success('Project deleted')
    notifyProjectSettingsRealtime('project-settings-deleted')
    router.replace('/spaces')
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.response?.data?.message || 'Could not delete project')
    }
  }
}

const goBack = () => {
  router.push(`/space/${projectId}`)
}

const goToModulesWorkspace = () => {
  router.push(`/space/${projectId}/modules`)
}

const goToCyclesWorkspace = () => {
  router.push(`/space/${projectId}/cycles`)
}

let unsubscribeAdminRealtime = null

onMounted(async () => {
  await loadProjectSettings()
  unsubscribeAdminRealtime = subscribeAdminRealtime(async ({ type, payload }) => {
    if (payload?.projectId && `${payload.projectId}` !== `${projectId}`) {
      return
    }

    if (type === 'project-settings-deleted') {
      router.replace('/spaces')
      return
    }

    if (
      [
        'project-settings-updated',
        'project-settings-favorite-updated',
        'project-settings-integrations-updated',
        'project-administration-updated'
      ].includes(type)
    ) {
      await loadProjectSettings()
    }
  })
})

onUnmounted(() => {
  unsubscribeAdminRealtime?.()
})
</script>

<style scoped>
.project-settings-page {
  min-height: calc(100vh - 66px);
  padding: 28px;
  color: #e4e4e7;
}

.settings-header,
.card-head,
.header-actions,
.danger-actions,
.meta-strip,
.row-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.settings-header {
  margin: 0 auto 24px;
  max-width: 1360px;
}

.breadcrumb {
  color: #60a5fa;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin-bottom: 8px;
}

.settings-header h1,
.card-head h2 {
  margin: 0;
}

.settings-header p,
.card-head p,
.row-main p {
  margin: 4px 0 0;
  color: #a1a1aa;
}

.settings-shell {
  max-width: 1360px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 260px minmax(0, 1fr);
  gap: 20px;
  align-items: start;
}

.settings-nav {
  position: sticky;
  top: 84px;
  display: grid;
  gap: 10px;
}

.nav-tab,
.settings-card,
.stack-row {
  background: #16181d;
  border: 1px solid #1f232a;
  border-radius: 14px;
}

.nav-tab {
  padding: 14px 16px;
  text-align: left;
  color: #d4d4d8;
  cursor: pointer;
  transition: 0.2s ease;
}

.nav-tab span,
.nav-tab small {
  display: block;
}

.nav-tab small {
  margin-top: 4px;
  color: #71717a;
}

.nav-tab.active {
  border-color: #38bdf8;
  background: linear-gradient(135deg, rgba(56, 189, 248, 0.16), rgba(15, 23, 42, 0.92));
}

.settings-content {
  display: grid;
}

.settings-card {
  padding: 24px;
}

.form-grid,
.inline-form,
.stack-list,
.editable-grid {
  display: grid;
  gap: 14px;
}

.editable-grid {
  grid-template-columns: repeat(3, minmax(0, 1fr));
  width: 100%;
}

.editable-grid.compact-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.integration-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.form-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
  margin-top: 20px;
}

.invite-grid,
.state-grid,
.label-grid,
.module-grid,
.cycle-grid {
  grid-template-columns: repeat(4, minmax(0, 1fr));
  margin-top: 20px;
  margin-bottom: 20px;
}

.module-grid,
.cycle-grid {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

label {
  display: grid;
  gap: 8px;
}

label.wide {
  grid-column: 1 / -1;
}

label span {
  color: #a1a1aa;
  font-size: 13px;
  font-weight: 600;
}

input,
textarea,
select {
  width: 100%;
  border: 1px solid #27272a;
  background: #0f1115;
  color: #fff;
  border-radius: 10px;
  padding: 10px 12px;
  font: inherit;
}

input:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.meta-strip {
  justify-content: flex-start;
  flex-wrap: wrap;
  margin-top: 20px;
  color: #94a3b8;
  font-size: 13px;
}

.meta-strip.compact {
  margin-top: 10px;
  gap: 10px;
}

.stack-row {
  padding: 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.row-main {
  min-width: 0;
  flex: 1;
}

.row-actions {
  flex-shrink: 0;
}

.state-row {
  display: grid;
  grid-template-columns: minmax(180px, 1fr) 72px 92px;
  gap: 10px;
  align-items: center;
}

.label-chip {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.color-dot {
  width: 12px;
  height: 12px;
  border-radius: 999px;
}

.primary-btn,
.secondary-btn,
.danger-btn,
.danger-outline-btn {
  min-height: 42px;
  padding: 0 14px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 600;
  font: inherit;
}

.primary-btn {
  background: #0ea5e9;
  color: #fff;
  border: 1px solid #0284c7;
}

.secondary-btn,
.danger-outline-btn {
  background: transparent;
  color: #e4e4e7;
  border: 1px solid #27272a;
}

.danger-btn {
  background: #b91c1c;
  color: #fff;
  border: 1px solid #ef4444;
}

.danger-outline-btn {
  color: #fca5a5;
  border-color: rgba(239, 68, 68, 0.4);
}

.danger-card {
  border-color: rgba(239, 68, 68, 0.25);
}

.danger-actions {
  justify-content: flex-start;
  margin-top: 20px;
}

.integration-actions {
  gap: 12px;
}

.empty-state {
  padding: 18px;
  border-radius: 10px;
  border: 1px dashed #334155;
  color: #94a3b8;
}

.section-split {
  margin: 18px 0 12px;
}

.section-split h3 {
  margin: 0;
  font-size: 14px;
}

.section-split p {
  margin: 6px 0 0;
  color: #94a3b8;
  font-size: 13px;
}

.helper-panel {
  margin-top: 16px;
}

@media (max-width: 1100px) {
  .settings-shell {
    grid-template-columns: 1fr;
  }

  .settings-nav {
    position: static;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .invite-grid,
  .state-grid,
  .label-grid,
  .module-grid,
  .cycle-grid,
  .form-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 760px) {
  .project-settings-page {
    padding: 18px;
  }

  .settings-header,
  .header-actions,
  .card-head,
  .stack-row,
  .row-actions,
  .danger-actions {
    flex-direction: column;
    align-items: flex-start;
  }

  .settings-nav {
    grid-template-columns: 1fr;
  }

  .state-row {
    grid-template-columns: 1fr;
  }
}
</style>
