<template>
  <AdminLayout>
    <div class="directory-page" :class="{ 'is-dimmed': showInvitePanel || showExportModal }" @click="closePageMenus">
      <div class="page-header">
        <div class="header-copy">
          <div class="breadcrumb">
            <i class="fa-solid fa-users"></i>
            <span>{{ t('Admin / User Directory', 'Quản trị / Danh sách Người dùng') }}</span>
          </div>
          <h1 class="page-title">{{ t('Users', 'Người dùng') }}</h1>
          <p class="page-subtitle">
            {{ t('Manage accounts, invitations, and access permissions for members in the organization.', 'Quản lý tài khoản, lời mời và quyền truy cập của thành viên trong tổ chức.') }}
            <a href="#" @click.prevent="openAppAccessHint">{{ t('Go to app access settings', 'Đi tới cài đặt quyền truy cập ứng dụng') }}</a>
          </p>
        </div>

        <div class="page-actions">
          <button type="button" class="primary-btn" @click.stop="openInvitePanel">
            <i class="fa-solid fa-user-plus"></i>
            {{ t('Invite users', 'Mời người dùng') }}
          </button>
          <button type="button" class="neutral-btn" @click="handleApproveRequests">
            {{ t('Approve requests', 'Duyệt yêu cầu') }}
            <span class="request-count">0</span>
          </button>
          <div class="menu-wrap" @click.stop>
            <button
              type="button"
              class="icon-btn"
              :class="{ 'is-open': openTopActionMenu }"
              @click="openTopActionMenu = !openTopActionMenu"
              aria-label="More actions"
            >
              <i class="fa-solid fa-ellipsis"></i>
            </button>
            <div v-if="openTopActionMenu" class="action-menu top-action-menu">
              <button type="button" @click="openExportModal">{{ t('Export users', 'Xuất danh sách') }}</button>
            </div>
          </div>
        </div>
      </div>

      <section class="content-frame">
        <div class="stats-grid">
          <div class="metric-card">
            <span>{{ t('Total users', 'Tổng số người dùng') }}</span>
            <strong>{{ users.length }}</strong>
          </div>
          <div class="metric-card">
            <span>{{ t('Active users', 'Đang hoạt động') }}</span>
            <strong>{{ activeUsersCount }}</strong>
          </div>
          <div class="metric-card">
            <span>{{ t('Organization admins', 'Quản trị viên') }}</span>
            <strong>{{ adminUsersCount }}</strong>
          </div>
        </div>

        <section class="filters-row" aria-label="User filters">
          <div class="table-search">
            <i class="fa-solid fa-magnifying-glass"></i>
            <input
              v-model="searchQuery"
              type="text"
              :placeholder="t('Search by name or email', 'Tìm theo tên hoặc email')"
              @input="debounceSearch"
            />
          </div>

          <div class="filter-wrap" @click.stop>
            <button
              type="button"
              class="filter-button"
              :class="{ 'is-open': openFilterMenu === 'role', 'has-value': roleFilterValues.length }"
              @click="toggleFilterMenu('role')"
            >
              {{ t('Role', 'Vai trò') }}
              <i class="fa-solid fa-chevron-down"></i>
            </button>
            <div v-if="openFilterMenu === 'role'" class="filter-menu role-filter-menu">
              <div class="filter-search">
                <input v-model="filterSearch.role" type="text" :placeholder="t('Search', 'Tìm kiếm')" />
                <i class="fa-solid fa-magnifying-glass"></i>
              </div>
              <label v-for="option in visibleRoleFilterOptions" :key="option.value" class="check-option">
                <input v-model="roleFilterValues" type="checkbox" :value="option.value" />
                <span>{{ option.label }}</span>
              </label>
              <div class="filter-menu-footer">{{ roleFilterOptions.length }} of {{ roleFilterOptions.length }}</div>
            </div>
          </div>

          <!-- <div class="filter-wrap" @click.stop>
            <button
              type="button"
              class="filter-button"
              :class="{ 'is-open': openFilterMenu === 'apps', 'has-value': appFilterValues.length }"
              @click="toggleFilterMenu('apps')"
            >
              {{ t('Apps', 'Ứng dụng') }}
              <i class="fa-solid fa-chevron-down"></i>
            </button>
            <div v-if="openFilterMenu === 'apps'" class="filter-menu apps-filter-menu">
              <div class="filter-search">
                <input v-model="filterSearch.apps" type="text" :placeholder="t('Search', 'Tìm kiếm')" />
                <i class="fa-solid fa-magnifying-glass"></i>
              </div>
              <p class="filter-org-label">{{ organizationHandle }}</p>
              <label v-for="option in visibleAppFilterOptions" :key="option.value" class="app-check-option">
                <input v-model="appFilterValues" type="checkbox" :value="option.value" />
                <span class="mini-app-icon" :class="option.tone">
                  <i class="fa-solid" :class="option.icon"></i>
                </span>
                <span>
                  <strong>{{ option.label }}</strong>
                  <small>{{ organizationHandle }}</small>
                </span>
              </label>
              <div class="filter-menu-footer">{{ appFilterOptions.length }} of {{ appFilterOptions.length }}</div>
            </div>
          </div> -->

          <div class="filter-wrap" @click.stop>
            <button
              type="button"
              class="filter-button"
              :class="{ 'is-open': openFilterMenu === 'status', 'has-value': statusFilterValues.length }"
              @click="toggleFilterMenu('status')"
            >
              {{ t('Status', 'Trạng thái') }}
              <i class="fa-solid fa-chevron-down"></i>
            </button>
            <div v-if="openFilterMenu === 'status'" class="filter-menu status-filter-menu">
              <div class="filter-search">
                <input v-model="filterSearch.status" type="text" :placeholder="t('Search', 'Tìm kiếm')" />
                <i class="fa-solid fa-magnifying-glass"></i>
              </div>
              <label v-for="option in visibleStatusFilterOptions" :key="option.value" class="check-option">
                <input v-model="statusFilterValues" type="checkbox" :value="option.value" />
                <span class="status-sample" :class="option.className">{{ option.label }}</span>
              </label>
              <div class="filter-menu-footer">{{ statusFilterOptions.length }} of {{ statusFilterOptions.length }}</div>
            </div>
          </div>

          <button v-if="hasActiveFilters" type="button" class="clear-filters-btn" @click="clearFilters">
            {{ t('Clear filters', 'Xóa bộ lọc') }}
          </button>
        </section>

        <div class="results-label">
          {{ t('Showing results', 'Hiển thị kết quả') }}
          <i v-if="loading" class="fa-solid fa-spinner fa-spin"></i>
        </div>

        <section class="users-table-shell">
          <div v-if="loading && !users.length" class="table-state">
            <i class="fa-solid fa-spinner fa-spin"></i>
            <span>{{ t('Loading users...', 'Đang tải người dùng...') }}</span>
          </div>

          <div v-else-if="!filteredUsers.length" class="table-state">
            <i class="fa-regular fa-user"></i>
            <span>{{ t('No users match these filters.', 'Không có người dùng nào phù hợp.') }}</span>
            <button type="button" class="primary-btn" @click.stop="openInvitePanel">{{ t('Invite users', 'Mời người dùng') }}</button>
          </div>

          <table v-else class="users-table">
            <thead>
              <tr>
                <th>{{ t('User', 'Người dùng') }}</th>
                <th>{{ t('Status', 'Trạng thái') }}</th>
                <th>{{ t('Last seen', 'Lần cuối truy cập') }}</th>
                <th class="actions-heading">{{ t('Actions', 'Thao tác') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in filteredUsers" :key="user.id || user.email">
                <td>
                  <div class="user-cell">
                    <div class="user-avatar" :style="{ backgroundColor: getAvatarColor(getDisplayName(user)) }">
                      {{ getInitials(getDisplayName(user)) }}
                    </div>
                    <div class="user-copy">
                      <strong>{{ getDisplayName(user) }}</strong>
                      <span>{{ user.email }} <template v-if="isOrganizationAdmin(user)">- {{ t('Organization admin', 'Quản trị viên') }}</template></span>
                    </div>
                  </div>
                </td>
                <td>
                  <span class="table-status-badge" :class="getStatusMeta(user).className">
                    {{ getStatusMeta(user).label }}
                  </span>
                </td>
                <td class="last-seen-cell">{{ getLastSeenText(user) }}</td>
                <td class="actions-cell">
                  <div class="menu-wrap row-menu-wrap" @click.stop>
                    <button
                      type="button"
                      class="row-more-btn"
                      :class="{ 'is-open': openRowMenu === (user.id || user.email) }"
                      @click="toggleRowMenu(user)"
                      aria-label="User actions"
                    >
                      <i class="fa-solid fa-ellipsis"></i>
                    </button>
                    <div v-if="openRowMenu === (user.id || user.email)" class="action-menu row-action-menu">
                      <button v-if="getStatusMeta(user).value === 'invited'" type="button" @click="resendInvite(user)">
                        {{ t('Resend invitation', 'Gửi lại lời mời') }}
                      </button>
                      <button type="button" @click="showAddGroupHint(user)">{{ t('Add to project', 'Thêm vào dự án') }}</button>
                      <button v-if="user.isActive" type="button" @click="handleSuspendUser(user)">
                        {{ t('Suspend access', 'Tạm ngưng truy cập') }}
                      </button>
                      <button type="button" class="danger-action" @click="handleRemoveUser(user)">
                        {{ t('Remove user', 'Xóa người dùng') }}
                      </button>
                    </div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </section>
      </section>
    </div>

    <section v-if="showInvitePanel" class="invite-panel">
      <button class="close-invite" type="button" aria-label="Close invite panel" @click="closeInvitePanel">
        <i class="fa-solid fa-xmark"></i>
      </button>

      <div class="invite-shell" @click.stop="closeInviteFloatingMenus">
        <div class="invite-copy">
          <h2>{{ t(`Invite people to ${organizationHandle}`, `Mời người đến ${organizationHandle}`) }}</h2>
          <p>
            {{ t('Invite teammates to collaborate and use apps in your organization. We\'ll ask new users to enter their personal details when they sign up.', 'Mời đồng đội cộng tác và sử dụng ứng dụng trong tổ chức của bạn. Chúng tôi sẽ yêu cầu người dùng mới nhập thông tin cá nhân khi đăng ký.') }}
          </p>
        </div>

        <div class="invite-section">
          <label class="field-label" for="invite-emails">{{ t('Email addresses', 'Địa chỉ email') }}</label>
          <div class="email-composer" :class="{ 'is-focused': emailInputFocused }" @click.stop>
            <span v-for="email in inviteEmails" :key="email" class="email-chip">
              {{ email }}
              <button type="button" @click="removeInviteEmail(email)">
                <i class="fa-solid fa-xmark"></i>
              </button>
            </span>
            <input
              id="invite-emails"
              v-model="inviteDraft"
              type="text"
              placeholder="name@company.com"
              @focus="emailInputFocused = true"
              @blur="handleEmailBlur"
              @keydown.enter.prevent="addDraftEmails"
              @paste="handleEmailPaste"
            />
          </div>
          <p class="hint">{{ t('Separate emails using a comma. Note, we can\'t send invitations to distribution lists.', 'Phân cách email bằng dấu phẩy. Lưu ý, chúng tôi không thể gửi lời mời đến danh sách phân phối.') }}</p>
        </div>

        <!-- <div class="access-table" v-show="false">
          <div class="access-header">
            <span>{{ t('App', 'Ứng dụng') }}</span>
            <span>{{ t('Plan', 'Gói') }}</span>
            <span>{{ t('Roles', 'Vai trò') }}</span>
            <button type="button" @click="clearAppAccess">{{ t('Unselect all', 'Bỏ chọn tất cả') }}</button>
          </div>

          <div v-for="row in appAccessRows" :key="row.key" class="access-row">
            <div class="app-cell">
              <div class="app-icon" :class="row.tone">
                <i class="fa-solid" :class="row.icon"></i>
              </div>
              <div>
                <strong>{{ row.name }}</strong>
                <span>{{ organizationHandle }}</span>
              </div>
            </div>
            <span class="plan-cell">{{ row.plan }}</span>
            <div class="role-picker" @click.stop>
              <button
                type="button"
                class="role-trigger"
                :class="{ 'is-open': openRoleMenu === row.key }"
                @click="toggleRoleMenu(row.key)"
              >
                <span>{{ getSelectedAppOption(row)?.label || 'None' }}</span>
                <i class="fa-solid fa-chevron-down"></i>
              </button>

              <div v-if="openRoleMenu === row.key" class="role-menu">
                <button
                  v-for="option in row.options"
                  :key="option.value"
                  type="button"
                  class="role-option"
                  :class="{ 'is-selected': inviteForm.apps[row.key] === option.value }"
                  @click="selectAppRole(row.key, option.value)"
                >
                  <span class="check-box">
                    <i v-if="inviteForm.apps[row.key] === option.value" class="fa-solid fa-check"></i>
                  </span>
                  <span class="role-option-copy">
                    <strong>{{ option.label }}</strong>
                    <small>{{ option.description }}</small>
                  </span>
                </button>
                <div class="role-menu-footer">
                  <button type="button" @click="selectAppRole(row.key, 'None')">{{ t('Unselect all', 'Bỏ chọn tất cả') }}</button>
                </div>
              </div>
            </div>
          </div>
        </div> -->

        <!-- <div class="invite-section" v-show="false">
          <label class="field-label">{{ t('Group membership', 'Thành viên nhóm') }}</label>
          <div class="group-composer" :class="{ 'is-focused': groupInputFocused }" @click.stop>
            <span v-for="group in selectedGroups" :key="group.name" class="group-chip">
              {{ group.name }}
              <button type="button" @click="removeGroup(group.name)">
                <i class="fa-solid fa-xmark"></i>
              </button>
            </span>
            <input
              v-model="groupDraft"
              type="text"
              :placeholder="t('Add groups', 'Thêm nhóm')"
              @focus="groupInputFocused = true"
              @keydown.enter.prevent="addFirstMatchingGroup"
              @keydown.esc.prevent="groupInputFocused = false"
            />
          </div>

          <div v-if="groupInputFocused" class="group-suggestions" @click.stop>
            <button
              v-for="group in filteredGroupSuggestions"
              :key="group.name"
              type="button"
              class="group-option"
              @click="addGroup(group)"
            >
              <strong>{{ group.name }}</strong>
              <span>{{ group.description }}</span>
            </button>
          </div>
          <p class="hint">{{ t('Customized groups give users access to specific projects or spaces.', 'Nhóm tùy chỉnh cho phép người dùng truy cập vào các dự án hoặc không gian cụ thể.') }}</p>
        </div> -->

        <div class="invite-section">
          <label class="field-label">{{ t('Project membership', 'Thành viên dự án') }}</label>
          <div class="project-grid">
            <el-select
              v-model="inviteForm.projectId"
              class="project-select"
              clearable
              filterable
              :placeholder="t('Add to project', 'Thêm vào dự án')"
              popper-class="admin-project-dropdown"
            >
              <el-option
                v-for="project in projectsList"
                :key="project.id"
                :label="project.name"
                :value="project.id"
              ></el-option>
            </el-select>

            <el-select v-model="inviteForm.projectRole" class="project-role-select" popper-class="admin-project-dropdown">
              <el-option label="Developer" value="DEV"></el-option>
              <el-option label="Project Manager" value="PM"></el-option>
              <el-option label="Product Owner" value="PO"></el-option>
              <el-option label="QA" value="QA"></el-option>
              <el-option label="Guest" value="Guest"></el-option>
            </el-select>
          </div>
          <p class="hint">{{ t('If a project is selected, the invited user will be added to it after accepting.', 'Nếu chọn dự án, người được mời sẽ được thêm vào sau khi chấp nhận.') }}</p>
        </div>

        <div class="invite-section">
          <button class="collapse-trigger" type="button" @click="showPersonalMessage = !showPersonalMessage">
            <i class="fa-solid" :class="showPersonalMessage ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
            {{ t('Personalize invitation email', 'Tùy chỉnh email mời') }}
          </button>

          <el-input
            v-if="showPersonalMessage"
            v-model="inviteForm.message"
            type="textarea"
            :rows="4"
            maxlength="300"
            show-word-limit
            :placeholder="t('Add a short message for the invite email...', 'Thêm lời nhắn ngắn cho email mời...')"
          />
        </div>

        <div class="invite-footer">
          <div>
            <!-- <strong v-show="false">{{ selectedAppsCount }} {{ t('apps selected', 'ứng dụng đã chọn') }}</strong> -->
            <p>{{ t('The invite email will include an accept link for the selected people.', 'Email mời sẽ bao gồm liên kết chấp nhận cho những người được chọn.') }}</p>
          </div>

          <div class="footer-actions">
            <button type="button" class="plain-action" @click="closeInvitePanel">{{ t('Cancel', 'Hủy') }}</button>
            <button
              type="button"
              class="primary-btn"
              :disabled="!canSendInvite"
              @click="submitInvite"
            >
              <i v-if="isSubmitting" class="fa-solid fa-spinner fa-spin"></i>
              <span>{{ t('Send invite', 'Gửi lời mời') }}</span>
            </button>
          </div>
        </div>
      </div>
    </section>

    <div v-if="showExportModal" class="modal-backdrop" @click.stop="closeExportModal">
      <section class="export-modal" @click.stop>
        <h2>{{ t('Export users to CSV', 'Xuất người dùng ra CSV') }}</h2>
        <p>
          {{ t('Select the data you want to export. We\'ll create the CSV file from the users currently loaded here.', 'Chọn dữ liệu bạn muốn xuất. Chúng tôi sẽ tạo file CSV từ danh sách người dùng hiện tại.') }}
          <a href="#">{{ t('Understand the CSV file', 'Tìm hiểu về file CSV') }}</a>
        </p>

        <div class="export-group">
          <span class="export-label">{{ t('Users', 'Người dùng') }}</span>
          <label><input v-model="exportUserScope" type="radio" value="all" /> {{ t('All users', 'Tất cả người dùng') }}</label>
          <label><input v-model="exportUserScope" type="radio" value="filtered" /> {{ t('Only users in selected filters', 'Chỉ người dùng trong bộ lọc') }}</label>
        </div>

        <div class="export-group">
          <span class="export-label">{{ t('User status', 'Trạng thái') }}</span>
          <label><input v-model="exportStatusScope" type="radio" value="all" /> {{ t('All users', 'Tất cả người dùng') }}</label>
          <label><input v-model="exportStatusScope" type="radio" value="active" /> {{ t('Only active users', 'Chỉ người dùng đang hoạt động') }}</label>
        </div>

        <div class="export-group">
          <span class="export-label">{{ t('Additional data', 'Dữ liệu bổ sung') }}</span>
          <label><input v-model="exportAdditionalData" type="checkbox" value="groups" /> {{ t('Group membership', 'Thành viên nhóm') }}</label>
          <label><input v-model="exportAdditionalData" type="checkbox" value="apps" /> {{ t('App access and role', 'Quyền truy cập ứng dụng và vai trò') }}</label>
        </div>

        <div class="modal-actions">
          <button type="button" class="plain-action" @click="closeExportModal">{{ t('Cancel', 'Hủy') }}</button>
          <button type="button" class="primary-btn" @click="exportUsersToCsv">{{ t('Export', 'Xuất') }}</button>
        </div>
      </section>
    </div>
  </AdminLayout>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { ElMessage, ElMessageBox } from 'element-plus'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import axiosClient from '@/api/axiosClient'
import { useAdminUserStore } from '@/store/useAdminUserStore'
import { useLocale } from '@/composables/useLocale'
import { onUnmounted } from 'vue'

const { t, locale: currentLocale } = useLocale()
const nowTime = ref(Date.now())
let timeInterval = null

const adminUserStore = useAdminUserStore()
const { users, loading } = storeToRefs(adminUserStore)

const fallbackOrganization = {
  organizationName: 'Global Organization',
  domain: 'acme.com',
  logoUrl: ''
}

const organizationProfile = ref({ ...fallbackOrganization })
const showInvitePanel = ref(false)
const showExportModal = ref(false)
const showPersonalMessage = ref(false)
const isSubmitting = ref(false)
const projectsList = ref([])
const searchQuery = ref('')
const inviteEmails = ref([])
const inviteDraft = ref('')
const emailInputFocused = ref(false)
const openRoleMenu = ref(null)
const groupDraft = ref('')
const groupInputFocused = ref(false)
const selectedGroups = ref([])
const openFilterMenu = ref(null)
const openTopActionMenu = ref(false)
const openRowMenu = ref(null)
const roleFilterValues = ref([])
const appFilterValues = ref([])
const statusFilterValues = ref([])
const filterSearch = ref({ role: '', apps: '', status: '' })
const exportUserScope = ref('all')
const exportStatusScope = ref('active')
const exportAdditionalData = ref([])

const inviteForm = ref({
  projectId: null,
  projectRole: 'DEV',
  message: '',
  apps: {
    goals: 'User',
    work: 'User',
    projects: 'User',
    admin: 'None'
  }
})

const appAccessRows = [
  {
    key: 'goals',
    name: 'Goals',
    plan: 'Free',
    icon: 'fa-bullseye',
    tone: 'tone-neutral',
    options: [
      { label: 'App admin', value: 'AppAdmin', description: 'Can access the app, with app admin permissions' },
      { label: 'User', value: 'User', description: 'Can access the app, with no app admin permissions' },
      { label: 'User access admin', value: 'AccessAdmin', description: 'No app access. Can administer users and groups for this app' },
      { label: 'None', value: 'None', description: 'No access to Goals' }
    ]
  },
  {
    key: 'work',
    name: 'Jira',
    plan: 'Premium',
    icon: 'fa-bolt',
    tone: 'tone-blue',
    options: [
      { label: 'User', value: 'User', description: 'Can access the app, with no app admin permissions' },
      { label: 'User access admin', value: 'AccessAdmin', description: 'No app access. Can administer users and groups for this app in administration' },
      { label: 'None', value: 'None', description: 'No access to Jira' }
    ]
  },
  {
    key: 'projects',
    name: 'Projects',
    plan: 'Free',
    icon: 'fa-rocket',
    tone: 'tone-dark',
    options: [
      { label: 'User access admin', value: 'AccessAdmin', description: 'No app access. Can administer users and groups for Projects' },
      { label: 'App admin', value: 'AppAdmin', description: 'Can access the app, with app admin permissions' },
      { label: 'User', value: 'User', description: 'Can access the app, with no app admin permissions' },
      { label: 'None', value: 'None', description: 'No access to Projects' }
    ]
  },
  {
    key: 'admin',
    name: 'Jira Administration',
    plan: 'Internal',
    icon: 'fa-shield-halved',
    tone: 'tone-blue',
    options: [
      { label: 'None', value: 'None', description: 'No administration access' },
      { label: 'App admin', value: 'AppAdmin', description: 'Can access the administration features for all Jira apps' }
    ]
  }
]

const roleFilterOptions = [
  { value: 'organization-admin', label: 'Organization admin' },
  { value: 'site-admin', label: 'Site admin' },
  { value: 'user-access-admin', label: 'User access admin' },
  { value: 'app-admin', label: 'App admin' },
  { value: 'user', label: 'User' },
  { value: 'guest', label: 'Guest' },
  { value: 'jsm-customer', label: 'Jira Service Management customer' }
]

const appFilterOptions = [
  { value: 'Goals', label: 'Goals', icon: 'fa-bullseye', tone: 'tone-neutral' },
  { value: 'Jira Administration', label: 'Jira Administration', icon: 'fa-bolt', tone: 'tone-blue' },
  { value: 'Jira', label: 'Jira', icon: 'fa-bolt', tone: 'tone-blue' },
  { value: 'Projects', label: 'Projects', icon: 'fa-rocket', tone: 'tone-dark' }
]

const statusFilterOptions = [
  { value: 'active', label: 'ACTIVE', className: 'status-active' },
  { value: 'invited', label: 'INVITED', className: 'status-invited' },
  { value: 'suspended', label: 'SUSPENDED', className: 'status-suspended' },
  { value: 'deactivated', label: 'DEACTIVATED', className: 'status-deactivated' }
]

const avatarColors = ['#579dff', '#c97cf4', '#00b8d9', '#22a06b', '#f5cd47', '#e2483d', '#6e5dc6', '#4bce97']

const organizationHandle = computed(() => makeOrganizationHandle(organizationProfile.value.domain || organizationProfile.value.organizationName))

const activeUsersCount = computed(() => users.value.filter(user => getStatusMeta(user).value === 'active').length)

const adminUsersCount = computed(() => users.value.filter(user => isOrganizationAdmin(user)).length)

const filteredUsers = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return users.value.filter(user => {
    const status = getStatusMeta(user).value
    const displayName = getDisplayName(user).toLowerCase()
    const email = (user.email || '').toLowerCase()
    const roles = getRoleKeys(user)
    const apps = getUserApps(user)

    const matchesSearch = !query || displayName.includes(query) || email.includes(query)
    const matchesRole = !roleFilterValues.value.length || roleFilterValues.value.some(role => roles.includes(role))
    const matchesApp = !appFilterValues.value.length || appFilterValues.value.some(app => apps.includes(app))
    const matchesStatus = !statusFilterValues.value.length || statusFilterValues.value.includes(status)

    return matchesSearch && matchesRole && matchesApp && matchesStatus
  })
})

const selectedAppsCount = computed(() =>
  Object.values(inviteForm.value.apps).filter(role => role !== 'None').length
)

const groupSuggestions = computed(() => {
  const handle = organizationHandle.value
  return [
    { name: `goals-admins-${handle}`, description: `Grants access to Goals and Goals administration features on ${handle}` },
    { name: `goals-user-access-admins-${handle}`, description: `Grants access to administer users and groups for Goals on ${handle}. Does not grant any product access.` },
    { name: `goals-users-${handle}`, description: `Grants access to Goals on ${handle}` },
    { name: `jira-admins-${handle}`, description: `Grants access to the administration features for all Jira products on ${handle}` },
    { name: `jira-user-access-admins-${handle}`, description: `Grants access to administer users and groups for Jira on ${handle}. Does not grant any product access.` },
    { name: `jira-users-${handle}`, description: `Grants access to Jira on ${handle}` },
    { name: 'org-admins', description: 'Grants access to administer org-level settings, users, groups, and billing' },
    { name: `projects-admins-${handle}`, description: `Grants access to Projects and Projects administration features on ${handle}` },
    { name: `projects-user-access-admins-${handle}`, description: `Grants access to administer users and groups for Projects on ${handle}. Does not grant any product access.` },
    { name: `projects-users-${handle}`, description: `Grants access to Projects on ${handle}` }
  ]
})

const filteredGroupSuggestions = computed(() => {
  const query = groupDraft.value.trim().toLowerCase()
  const selectedNames = new Set(selectedGroups.value.map(group => group.name))
  return groupSuggestions.value
    .filter(group => !selectedNames.has(group.name))
    .filter(group => !query || group.name.toLowerCase().includes(query) || group.description.toLowerCase().includes(query))
})

const visibleRoleFilterOptions = computed(() => filterOptions(roleFilterOptions, filterSearch.value.role))
const visibleAppFilterOptions = computed(() => filterOptions(appFilterOptions, filterSearch.value.apps))
const visibleStatusFilterOptions = computed(() => filterOptions(statusFilterOptions, filterSearch.value.status))

const draftHasValidEmail = computed(() =>
  inviteDraft.value
    .split(/[,\n;\s]+/)
    .map(email => email.trim().toLowerCase())
    .some(email => isValidEmail(email))
)

const canSendInvite = computed(() =>
  (inviteEmails.value.length > 0 || draftHasValidEmail.value) && selectedAppsCount.value > 0 && !isSubmitting.value
)

const hasActiveFilters = computed(() =>
  roleFilterValues.value.length > 0 || appFilterValues.value.length > 0 || statusFilterValues.value.length > 0
)

let searchTimeout = null

const filterOptions = (options, query) => {
  const normalized = query.trim().toLowerCase()
  if (!normalized) return options
  return options.filter(option => option.label.toLowerCase().includes(normalized))
}

const debounceSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    adminUserStore.fetchUsers(searchQuery.value)
  }, 350)
}

const loadOrganizationProfile = async () => {
  try {
    const res = await axiosClient.get('/settings/TenantProfile')
    const data = res.data?.data || {}
    organizationProfile.value = {
      organizationName: data.organizationName || fallbackOrganization.organizationName,
      domain: data.domain || fallbackOrganization.domain,
      logoUrl: data.logoUrl || ''
    }
  } catch (error) {
    organizationProfile.value = { ...fallbackOrganization }
  }
}

const makeOrganizationHandle = (value) => {
  const raw = String(value || 'sprinta').trim().toLowerCase()
  const withoutDomain = raw.includes('.') ? raw.split('.')[0] : raw
  return withoutDomain
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '') || 'sprinta'
}

const getAvatarColor = (name) => {
  if (!name) return avatarColors[0]
  let hash = 0
  for (let i = 0; i < name.length; i += 1) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

const getInitials = (name) => {
  if (!name) return '?'
  const cleanName = name.includes('@') ? name.split('@')[0] : name
  return cleanName
    .split(/[.\s_-]+/)
    .filter(Boolean)
    .map(part => part[0])
    .join('')
    .substring(0, 2)
    .toUpperCase()
}

const titleCase = (value) => {
  return String(value || '')
    .replace(/[._-]+/g, ' ')
    .split(' ')
    .filter(Boolean)
    .map(part => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ')
}

const getDisplayName = (user) => {
  return user.name || user.fullName || titleCase((user.email || '').split('@')[0]) || 'Unknown user'
}

const getNormalizedRoles = (user) => (user.roles || []).map(role => String(role).toLowerCase())

const isOrganizationAdmin = (user) => {
  return getNormalizedRoles(user).some(role => ['admin', 'system admin', 'superadmin', 'organization admin'].includes(role))
}

const getRoleKeys = (user) => {
  const roles = getNormalizedRoles(user)
  const keys = new Set()

  if (!roles.length || roles.some(role => ['member', 'user', 'developer', 'dev'].includes(role))) keys.add('user')
  if (roles.some(role => role.includes('guest'))) keys.add('guest')
  if (roles.some(role => role.includes('access admin'))) keys.add('user-access-admin')
  if (roles.some(role => role.includes('app admin') || role === 'admin')) keys.add('app-admin')
  if (roles.some(role => role.includes('site admin') || role === 'admin')) keys.add('site-admin')
  if (isOrganizationAdmin(user)) keys.add('organization-admin')

  return Array.from(keys)
}

const getUserApps = (user) => {
  const roles = getNormalizedRoles(user)
  if (isOrganizationAdmin(user)) return ['Goals', 'Jira Administration', 'Jira', 'Projects']
  if (roles.some(role => ['developer', 'dev', 'pm', 'po', 'qa'].includes(role))) return ['Jira', 'Projects']
  return ['Projects']
}

const getStatusMeta = (user) => {
  const rawStatus = String(user.status || '').toLowerCase()

  if (rawStatus.includes('invited') || rawStatus.includes('pending')) {
    return { label: t('INVITED', 'ĐÃ MỜI'), value: 'invited', className: 'status-invited' }
  }
  if (rawStatus.includes('suspend')) {
    return { label: t('SUSPENDED', 'BỊ ĐÌNH CHỈ'), value: 'suspended', className: 'status-suspended' }
  }
  if (rawStatus.includes('deactivate')) {
    return { label: t('DEACTIVATED', 'VÔ HIỆU HÓA'), value: 'deactivated', className: 'status-deactivated' }
  }
  if (user.isActive) {
    return { label: t('ACTIVE', 'HOẠT ĐỘNG'), value: 'active', className: 'status-active' }
  }

  return { label: t('SUSPENDED', 'BỊ ĐÌNH CHỈ'), value: 'suspended', className: 'status-suspended' }
}

const formatAtlassianDate = (value) => {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleDateString(currentLocale.value === 'vi' ? 'vi-VN' : 'en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

const getRelativeInviteTime = (value) => {
  if (!value) return t('Invited just now', 'Vừa mời xong')
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return t('Invited just now', 'Vừa mời xong')
  const minutes = Math.max(0, Math.floor((nowTime.value - date.getTime()) / 60000))
  if (minutes < 1) return t('Invited just now', 'Vừa mời xong')
  if (minutes < 60) return t(`Invited ${minutes} minutes ago`, `Đã mời ${minutes} phút trước`)
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return t(`Invited ${hours} hours ago`, `Đã mời ${hours} giờ trước`)
  return t(`Invited ${Math.floor(hours / 24)} days ago`, `Đã mời ${Math.floor(hours / 24)} ngày trước`)
}

const getLastSeenText = (user) => {
  const status = getStatusMeta(user).value
  if (status === 'invited') return getRelativeInviteTime(user.createdAt || user.invitedAt)
  return formatAtlassianDate(user.lastSeenAt || user.createdAt)
}

const toggleFilterMenu = (name) => {
  openTopActionMenu.value = false
  openRowMenu.value = null
  openFilterMenu.value = openFilterMenu.value === name ? null : name
}

const toggleRowMenu = (user) => {
  openFilterMenu.value = null
  openTopActionMenu.value = false
  const key = user.id || user.email
  openRowMenu.value = openRowMenu.value === key ? null : key
}

const clearFilters = () => {
  roleFilterValues.value = []
  appFilterValues.value = []
  statusFilterValues.value = []
  filterSearch.value = { role: '', apps: '', status: '' }
  openFilterMenu.value = null
}

const closePageMenus = () => {
  openFilterMenu.value = null
  openTopActionMenu.value = false
  openRowMenu.value = null
}

const closeInviteFloatingMenus = () => {
  openRoleMenu.value = null
  groupInputFocused.value = false
}

const resetInviteForm = () => {
  inviteEmails.value = []
  inviteDraft.value = ''
  groupDraft.value = ''
  selectedGroups.value = []
  groupInputFocused.value = false
  openRoleMenu.value = null
  showPersonalMessage.value = false
  inviteForm.value = {
    projectId: null,
    projectRole: 'DEV',
    message: '',
    apps: {
      goals: 'User',
      work: 'User',
      projects: 'User',
      admin: 'None'
    }
  }
}

const loadProjects = async () => {
  try {
    const res = await axiosClient.get('/projects')
    projectsList.value = res.data?.data || []
  } catch (error) {
    console.error('Failed to load projects', error)
  }
}

const openInvitePanel = async () => {
  closePageMenus()
  showInvitePanel.value = true
  if (!projectsList.value.length) {
    await loadProjects()
  }
}

const closeInvitePanel = () => {
  showInvitePanel.value = false
  emailInputFocused.value = false
  closeInviteFloatingMenus()
}

const openExportModal = () => {
  openTopActionMenu.value = false
  showExportModal.value = true
}

const closeExportModal = () => {
  showExportModal.value = false
}

const openAppAccessHint = () => {
  ElMessage.info(t('App access settings will use the same tenant profile and app roles.', 'Cài đặt quyền truy cập ứng dụng sẽ sử dụng cùng hồ sơ tổ chức và vai trò.'))
}

const isValidEmail = (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)

const addEmailsFromText = (text) => {
  const values = text
    .split(/[,\n;\s]+/)
    .map(email => email.trim().toLowerCase())
    .filter(Boolean)

  const invalidEmails = values.filter(email => !isValidEmail(email))
  const validEmails = values.filter(email => isValidEmail(email))

  validEmails.forEach(email => {
    if (!inviteEmails.value.includes(email)) inviteEmails.value.push(email)
  })

  if (invalidEmails.length) {
    ElMessage.warning(t(`Invalid email: ${invalidEmails.join(', ')}`, `Email không hợp lệ: ${invalidEmails.join(', ')}`))
  }
}

const addDraftEmails = () => {
  if (!inviteDraft.value.trim()) return
  addEmailsFromText(inviteDraft.value)
  inviteDraft.value = ''
}

const handleEmailBlur = () => {
  emailInputFocused.value = false
  addDraftEmails()
}

const handleEmailPaste = (event) => {
  const pasted = event.clipboardData?.getData('text')
  if (!pasted || !/[,\n;\s]+/.test(pasted)) return
  event.preventDefault()
  addEmailsFromText(pasted)
}

const removeInviteEmail = (email) => {
  inviteEmails.value = inviteEmails.value.filter(item => item !== email)
}

const toggleRoleMenu = (key) => {
  groupInputFocused.value = false
  openRoleMenu.value = openRoleMenu.value === key ? null : key
}

const selectAppRole = (key, value) => {
  inviteForm.value.apps[key] = value
  openRoleMenu.value = null
}

const getSelectedAppOption = (row) => {
  return row.options.find(option => option.value === inviteForm.value.apps[row.key])
}

const addGroup = (group) => {
  if (!selectedGroups.value.some(item => item.name === group.name)) {
    selectedGroups.value.push(group)
  }
  groupDraft.value = ''
  groupInputFocused.value = false
}

const removeGroup = (name) => {
  selectedGroups.value = selectedGroups.value.filter(group => group.name !== name)
}

const addFirstMatchingGroup = () => {
  const firstMatch = filteredGroupSuggestions.value[0]
  if (firstMatch) addGroup(firstMatch)
}

const clearAppAccess = () => {
  Object.keys(inviteForm.value.apps).forEach(key => {
    inviteForm.value.apps[key] = 'None'
  })
  openRoleMenu.value = null
}

const resolveSystemRole = () => {
  const selectedRoles = Object.values(inviteForm.value.apps)
  if (selectedRoles.includes('AppAdmin') || inviteForm.value.apps.admin === 'AppAdmin') return 'Admin'
  if (inviteForm.value.apps.work !== 'None' || inviteForm.value.apps.projects !== 'None') return 'Developer'
  return 'Member'
}

const resolveUserSystemRole = (user) => {
  const roles = getNormalizedRoles(user)
  if (roles.some(role => role.includes('admin'))) return 'Admin'
  if (roles.some(role => ['developer', 'dev', 'pm', 'po', 'qa'].includes(role))) return 'Developer'
  return 'Member'
}

const submitInvite = async () => {
  addDraftEmails()

  if (!canSendInvite.value) {
    ElMessage.warning(t('Enter at least one email and select app access.', 'Vui lòng nhập ít nhất một email và chọn quyền ứng dụng.'))
    return
  }

  isSubmitting.value = true
  try {
    const role = resolveSystemRole()
    const payloads = inviteEmails.value.map(email => ({
      email,
      role,
      projectId: inviteForm.value.projectId,
      projectRole: inviteForm.value.projectRole,
      inviteMessage: inviteForm.value.message,
      inviteGroups: selectedGroups.value.map(group => group.name)
    }))

    await adminUserStore.inviteUsers(payloads)
    ElMessage.success(t(`Sent ${payloads.length} invitation email${payloads.length > 1 ? 's' : ''}.`, `Đã gửi ${payloads.length} email mời.`))
    closeInvitePanel()
    resetInviteForm()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('Could not send invitation. Please try again.', 'Không thể gửi lời mời. Vui lòng thử lại.'))
  } finally {
    isSubmitting.value = false
  }
}

const resendInvite = async (user) => {
  openRowMenu.value = null
  try {
    await adminUserStore.createUser({
      email: user.email,
      role: resolveUserSystemRole(user),
      projectId: null,
      projectRole: 'DEV',
      inviteMessage: ''
    })
    ElMessage.success(t(`Resent invitation to ${user.email}.`, `Đã gửi lại lời mời đến ${user.email}.`))
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('Could not resend invitation.', 'Không thể gửi lại lời mời.'))
  }
}

const handleSuspendUser = (user) => {
  openRowMenu.value = null
  if (!user.isActive) return

  ElMessageBox.confirm(
    t(`Suspend access for ${getDisplayName(user)}?`, `Tạm ngưng truy cập cho ${getDisplayName(user)}?`),
    t('Suspend user', 'Tạm ngưng người dùng'),
    {
      confirmButtonText: t('Suspend', 'Tạm ngưng'),
      cancelButtonText: t('Cancel', 'Hủy'),
      type: 'warning'
    }
  ).then(async () => {
    try {
      await adminUserStore.suspendUser(user.id)
      ElMessage.success(t(`Suspended ${getDisplayName(user)}.`, `Đã tạm ngưng ${getDisplayName(user)}.`))
    } catch (error) {
      ElMessage.error(error.response?.data?.message || t('Could not suspend user.', 'Không thể tạm ngưng người dùng.'))
    }
  }).catch(() => {})
}

const handleApproveRequests = () => {
  ElMessage.info(t('No pending requests to approve.', 'Không có yêu cầu nào cần duyệt.'))
}

const handleRemoveUser = (user) => {
  openRowMenu.value = null
  ElMessageBox.confirm(
    t(`Are you sure you want to completely remove ${getDisplayName(user)}? This action cannot be undone.`, `Bạn có chắc muốn xóa hoàn toàn ${getDisplayName(user)}? Thao tác này không thể hoàn tác.`),
    t('Remove user', 'Xóa người dùng'),
    {
      confirmButtonText: t('Remove', 'Xóa'),
      cancelButtonText: t('Cancel', 'Hủy'),
      type: 'error'
    }
  ).then(async () => {
    try {
      await adminUserStore.removeUser(user.id)
      ElMessage.success(t(`Removed ${getDisplayName(user)}.`, `Đã xóa ${getDisplayName(user)}.`))
    } catch (error) {
      ElMessage.error(error.response?.data?.message || t('Could not remove user.', 'Không thể xóa người dùng.'))
    }
  }).catch(() => {})
}

const showAddGroupHint = (user) => {
  openRowMenu.value = null
  openInvitePanel()
  ElMessage.info(t(`You can assign groups to ${getDisplayName(user)} via Invite menu.`, `Bạn có thể gán nhóm cho ${getDisplayName(user)} qua menu Mời.`))
}

const escapeCsv = (value) => {
  const text = String(value ?? '')
  if (/[",\n]/.test(text)) return `"${text.replace(/"/g, '""')}"`
  return text
}

const exportUsersToCsv = () => {
  let sourceUsers = exportUserScope.value === 'filtered' ? filteredUsers.value : users.value
  if (exportStatusScope.value === 'active') {
    sourceUsers = sourceUsers.filter(user => getStatusMeta(user).value === 'active')
  }

  const headers = ['Name', 'Email', 'Status', 'Roles', 'Last seen']
  if (exportAdditionalData.value.includes('apps')) headers.push('Apps')
  if (exportAdditionalData.value.includes('groups')) headers.push('Groups')

  const rows = sourceUsers.map(user => {
    const row = [
      getDisplayName(user),
      user.email,
      getStatusMeta(user).label,
      (user.roles || []).join('; ') || 'User',
      getLastSeenText(user)
    ]
    if (exportAdditionalData.value.includes('apps')) row.push(getUserApps(user).join('; '))
    if (exportAdditionalData.value.includes('groups')) row.push((user.groups || []).join('; '))
    return row
  })

  const csv = [headers, ...rows]
    .map(row => row.map(escapeCsv).join(','))
    .join('\n')

  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${organizationHandle.value}-users.csv`
  link.click()
  URL.revokeObjectURL(url)
  closeExportModal()
  ElMessage.success(t('CSV export ready.', 'Đã xuất file CSV thành công.'))
}

onMounted(async () => {
  timeInterval = setInterval(() => { nowTime.value = Date.now() }, 60000)
  await Promise.all([
    adminUserStore.fetchUsers(),
    loadOrganizationProfile()
  ])
})

onUnmounted(() => {
  if (timeInterval) clearInterval(timeInterval)
})
</script>

<style scoped>
.directory-page {
  position: relative;
  transition: opacity 0.18s ease;
}

.directory-page.is-dimmed {
  opacity: 0.22;
  pointer-events: none;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 24px;
  margin-bottom: 24px;
}

.header-copy {
  min-width: 0;
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
  color: var(--text-muted);
  font-size: 13px;
}

.page-title {
  margin: 0;
  color: var(--text-primary);
  font-size: 28px;
  font-weight: 700;
}

.page-subtitle {
  max-width: 760px;
  margin: 8px 0 0;
  color: var(--text-secondary);
  font-size: 14px;
  line-height: 1.6;
}

.page-subtitle a {
  color: #579dff;
  text-decoration: none;
}

.page-subtitle a:hover {
  text-decoration: underline;
}

.page-actions,
.filters-row,
.footer-actions,
.modal-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.page-actions {
  flex: 0 0 auto;
}

.content-frame {
  position: relative;
  overflow: visible;
  padding: 24px;
  border: 1px solid var(--border-color);
  border-radius: 12px;
  background: var(--bg-card);
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.18);
}

.primary-btn,
.neutral-btn,
.icon-btn,
.filter-button,
.plain-action,
.clear-filters-btn {
  min-height: 34px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  letter-spacing: 0;
}

.primary-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0 14px;
  border: 1px solid #579dff;
  background: #579dff;
  color: #081120;
}

.primary-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.neutral-btn,
.icon-btn,
.filter-button {
  border: 1px solid var(--border-color);
  background: rgba(255, 255, 255, 0.02);
  color: var(--text-primary);
}

.neutral-btn {
  padding: 0 12px;
}

.icon-btn {
  width: 34px;
  padding: 0;
}

.request-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 22px;
  height: 20px;
  margin-left: 8px;
  padding: 0 6px;
  border-radius: 6px;
  background: rgba(255, 255, 255, 0.08);
  color: var(--text-muted);
  font-size: 12px;
}

.menu-wrap {
  position: relative;
}

.icon-btn.is-open,
.row-more-btn.is-open,
.filter-button.is-open {
  border-color: #579dff;
  box-shadow: 0 0 0 1px rgba(87, 157, 255, 0.7);
}

.action-menu,
.filter-menu {
  position: absolute;
  z-index: 30;
  overflow: hidden;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: #1a1d24;
  box-shadow: 0 14px 28px rgba(0, 0, 0, 0.3);
}

.action-menu {
  min-width: 160px;
  padding: 6px 0;
}

.top-action-menu,
.row-action-menu {
  top: calc(100% + 8px);
  right: 0;
}

.action-menu button {
  display: block;
  width: 100%;
  min-height: 36px;
  padding: 0 14px;
  border: 0;
  background: transparent;
  color: var(--text-primary);
  cursor: pointer;
  font-size: 14px;
  text-align: left;
}

.action-menu button:hover {
  background: rgba(255, 255, 255, 0.05);
}

.danger-action {
  color: #f87171 !important;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 14px;
  margin-bottom: 22px;
}

.metric-card {
  min-height: 86px;
  padding: 16px;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.03);
}

.metric-card span {
  display: block;
  color: var(--text-muted);
  font-size: 13px;
}

.metric-card strong {
  display: block;
  margin-top: 10px;
  color: var(--text-primary);
  font-size: 28px;
  line-height: 1;
}

.filters-row {
  flex-wrap: wrap;
  margin-bottom: 16px;
  overflow: visible;
}

.filter-wrap {
  position: relative;
  overflow: visible;
}

.table-search,
.filter-search {
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-muted);
}

.table-search {
  width: 260px;
  min-height: 36px;
  padding: 0 10px;
}

.table-search input,
.filter-search input {
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-primary);
  font-size: 14px;
}

.filter-button {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
}

.filter-button.has-value {
  border-color: #579dff;
  color: #9cc4ff;
}

.filter-menu {
  top: calc(100% + 8px);
  left: 0;
  width: 250px;
  padding-top: 8px;
}

.apps-filter-menu {
  width: 292px;
}

.status-filter-menu {
  width: 184px;
}

.filter-search {
  min-height: 36px;
  margin: 0 8px 8px;
  padding: 0 8px;
}

.check-option,
.app-check-option {
  display: flex;
  align-items: center;
  gap: 10px;
  min-height: 30px;
  padding: 0 16px;
  color: var(--text-primary);
  cursor: pointer;
  font-size: 14px;
}

.check-option:hover,
.app-check-option:hover {
  background: rgba(255, 255, 255, 0.05);
}

.check-option input,
.app-check-option input,
.export-group input {
  width: 14px;
  height: 14px;
  margin: 0;
}

.filter-org-label {
  margin: 0;
  padding: 8px 16px 4px;
  color: var(--text-muted);
  font-size: 12px;
  font-weight: 700;
}

.app-check-option {
  align-items: center;
  min-height: 46px;
}

.app-check-option strong,
.app-check-option small {
  display: block;
}

.app-check-option strong {
  color: var(--text-primary);
  font-size: 14px;
  font-weight: 500;
}

.app-check-option small {
  margin-top: 2px;
  color: var(--text-muted);
  font-size: 12px;
}

.mini-app-icon,
.app-icon {
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: 6px;
}

.mini-app-icon {
  width: 20px;
  height: 20px;
  color: #ffffff;
  font-size: 10px;
}

.tone-neutral {
  background: #4b5563;
  color: #ffffff;
}

.tone-blue {
  background: #0c66e4;
  color: #ffffff;
}

.tone-dark {
  background: #172b4d;
  color: #ffffff;
}

.filter-menu-footer {
  min-height: 36px;
  padding: 10px 12px;
  border-top: 1px solid var(--border-color);
  color: var(--text-muted);
  font-size: 13px;
  text-align: right;
}

.status-sample,
.table-status-badge {
  display: inline-flex;
  align-items: center;
  min-height: 18px;
  padding: 0 6px;
  border: 1px solid currentColor;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 800;
  line-height: 1;
}

.status-active {
  color: #4bce97;
  background: rgba(34, 160, 107, 0.12);
}

.status-invited {
  color: #9cc4ff;
  background: rgba(87, 157, 255, 0.12);
}

.status-suspended {
  color: #f5cd47;
  background: rgba(245, 205, 71, 0.12);
}

.status-deactivated {
  color: #f87171;
  background: rgba(226, 72, 61, 0.12);
}

.clear-filters-btn {
  border: 0;
  background: transparent;
  color: #9cc4ff;
}

.results-label {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
  color: var(--text-primary);
  font-size: 15px;
}

.users-table-shell {
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.02);
}

.table-state {
  display: grid;
  place-items: center;
  gap: 12px;
  min-height: 220px;
  color: var(--text-muted);
  text-align: center;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
}

.users-table thead {
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.users-table th {
  height: 44px;
  padding: 0 16px;
  color: var(--text-muted);
  font-size: 13px;
  font-weight: 700;
  text-align: left;
}

.users-table td {
  padding: 12px 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.04);
  color: var(--text-primary);
  font-size: 14px;
  vertical-align: middle;
}

.users-table tbody tr:hover {
  background: rgba(255, 255, 255, 0.03);
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  color: #081120;
  font-size: 12px;
  font-weight: 800;
}

.user-copy strong,
.user-copy span {
  display: block;
}

.user-copy strong {
  color: var(--text-primary);
  font-size: 14px;
  font-weight: 600;
}

.user-copy span {
  margin-top: 2px;
  color: var(--text-muted);
  font-size: 12px;
}

.last-seen-cell {
  color: var(--text-secondary);
}

.actions-heading,
.actions-cell {
  text-align: right;
}

.row-menu-wrap {
  display: inline-block;
}

.row-more-btn {
  display: grid;
  place-items: center;
  width: 28px;
  height: 28px;
  border: 1px solid transparent;
  border-radius: 6px;
  background: transparent;
  color: var(--text-primary);
  cursor: pointer;
}

.row-more-btn:hover {
  border-color: var(--border-color);
  background: rgba(255, 255, 255, 0.04);
}

.invite-panel {
  position: fixed;
  inset: 0;
  z-index: 90;
  overflow-y: auto;
  background: #ffffff;
}

.close-invite {
  position: fixed;
  top: 28px;
  left: 28px;
  display: grid;
  place-items: center;
  width: 38px;
  height: 38px;
  border: 1px solid transparent;
  border-radius: 50%;
  background: #ffffff;
  color: #44546f;
  cursor: pointer;
  font-size: 18px;
}

.close-invite:hover {
  border-color: #0c66e4;
}

.invite-shell {
  width: min(920px, calc(100% - 48px));
  margin: 64px auto 96px;
}

.invite-copy {
  margin-bottom: 22px;
}

.invite-copy h2 {
  margin: 0;
  color: #172b4d;
  font-size: 25px;
  font-weight: 800;
}

.invite-copy p {
  max-width: 860px;
  margin: 8px 0 0;
  color: #172b4d;
  font-size: 14px;
  line-height: 1.5;
}

.invite-section {
  position: relative;
  margin-bottom: 20px;
}

.field-label {
  display: block;
  margin-bottom: 7px;
  color: #44546f;
  font-size: 12px;
  font-weight: 800;
}

.email-composer,
.group-composer {
  display: flex;
  min-height: 40px;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
  padding: 5px 8px;
  border: 1px solid #8590a2;
  border-radius: 4px;
  background: #ffffff;
}

.email-composer.is-focused,
.group-composer.is-focused {
  border-color: #0c66e4;
  box-shadow: 0 0 0 1px #0c66e4;
}

.email-composer input,
.group-composer input {
  min-width: 220px;
  flex: 1 1 220px;
  border: 0;
  outline: 0;
  color: #172b4d;
  font-size: 14px;
}

.email-chip,
.group-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-height: 26px;
  padding: 0 7px;
  border: 1px solid #dfe1e6;
  border-radius: 4px;
  background: #f1f2f4;
  color: #172b4d;
  font-size: 13px;
}

.email-chip button,
.group-chip button {
  display: grid;
  place-items: center;
  width: 18px;
  height: 18px;
  border: 0;
  border-radius: 4px;
  background: transparent;
  color: #626f86;
  cursor: pointer;
}

.hint {
  margin: 7px 0 0;
  color: #626f86;
  font-size: 12px;
}

.access-table {
  margin-bottom: 42px;
  border: 1px solid #dfe1e6;
  border-radius: 4px;
  background: #ffffff;
}

.access-header,
.access-row {
  display: grid;
  grid-template-columns: minmax(260px, 1.45fr) minmax(100px, 0.45fr) minmax(210px, 0.7fr) 90px;
  align-items: center;
  gap: 18px;
  min-height: 66px;
  padding: 0 10px;
}

.access-header {
  min-height: 40px;
  border-bottom: 1px solid #dfe1e6;
  color: #172b4d;
  font-size: 13px;
  font-weight: 800;
}

.access-header button {
  justify-self: end;
  border: 0;
  background: transparent;
  color: #0c66e4;
  cursor: pointer;
  font-weight: 700;
}

.access-row + .access-row {
  border-top: 1px solid #dfe1e6;
}

.app-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.app-icon {
  width: 32px;
  height: 32px;
}

.app-cell strong,
.app-cell span {
  display: block;
}

.app-cell strong {
  color: #172b4d;
  font-size: 14px;
  font-weight: 500;
}

.app-cell span,
.plan-cell {
  color: #44546f;
  font-size: 13px;
}

.role-picker,
.project-select,
.project-role-select {
  width: 100%;
}

.role-picker {
  position: relative;
}

.role-trigger {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  min-height: 34px;
  padding: 0 10px;
  border: 1px solid #dfe1e6;
  border-radius: 4px;
  background: #ffffff;
  color: #172b4d;
  cursor: pointer;
  font-size: 14px;
}

.role-trigger.is-open {
  border-color: #0c66e4;
  box-shadow: 0 0 0 1px #0c66e4;
}

.role-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  z-index: 40;
  width: 334px;
  overflow: hidden;
  border: 1px solid #dfe1e6;
  border-radius: 4px;
  background: #ffffff;
  box-shadow: 0 8px 20px rgba(9, 30, 66, 0.2);
}

.role-option {
  display: grid;
  grid-template-columns: 22px 1fr;
  gap: 10px;
  width: 100%;
  padding: 12px 16px;
  border: 0;
  background: #ffffff;
  color: #172b4d;
  cursor: pointer;
  text-align: left;
}

.role-option:hover,
.role-option.is-selected {
  background: #e9f2ff;
}

.check-box {
  display: grid;
  place-items: center;
  width: 14px;
  height: 14px;
  margin-top: 3px;
  border: 1px solid #a6adba;
  border-radius: 2px;
  color: #ffffff;
  font-size: 9px;
}

.role-option.is-selected .check-box {
  border-color: #0c66e4;
  background: #0c66e4;
}

.role-option-copy strong,
.role-option-copy small {
  display: block;
}

.role-option-copy strong {
  color: #172b4d;
  font-size: 14px;
  font-weight: 500;
  line-height: 1.3;
}

.role-option-copy small {
  margin-top: 4px;
  color: #44546f;
  font-size: 13px;
  line-height: 1.45;
}

.role-option.is-selected .role-option-copy small {
  color: #0c66e4;
}

.role-menu-footer {
  padding: 12px 16px;
  border-top: 1px solid #dfe1e6;
  background: #ffffff;
}

.role-menu-footer button {
  border: 0;
  background: transparent;
  color: #0c66e4;
  cursor: pointer;
  font-weight: 600;
}

.group-suggestions {
  max-height: 248px;
  overflow-y: auto;
  border: 1px solid #dfe1e6;
  border-top: 0;
  background: #ffffff;
  box-shadow: 0 8px 20px rgba(9, 30, 66, 0.18);
}

.group-option {
  display: block;
  width: 100%;
  padding: 9px 10px;
  border: 0;
  background: #ffffff;
  color: #172b4d;
  cursor: pointer;
  text-align: left;
}

.group-option:hover {
  background: #f1f2f4;
}

.group-option strong,
.group-option span {
  display: block;
}

.group-option strong {
  color: #172b4d;
  font-size: 13px;
  line-height: 1.35;
}

.group-option span {
  margin-top: 3px;
  color: #44546f;
  font-size: 13px;
  line-height: 1.4;
}

.project-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 220px;
  gap: 10px;
}

.collapse-trigger {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  border: 0;
  background: transparent;
  color: #172b4d;
  cursor: pointer;
  font-size: 14px;
  font-weight: 700;
}

.invite-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  margin-top: 28px;
}

.invite-footer strong {
  color: #172b4d;
  font-size: 14px;
}

.invite-footer p {
  margin: 6px 0 0;
  color: #44546f;
  font-size: 13px;
}

.plain-action {
  padding: 0 12px;
  border: 0;
  background: transparent;
  color: #44546f;
}

.modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 80;
  display: grid;
  place-items: start center;
  padding-top: 52px;
  background: rgba(9, 30, 66, 0.56);
}

.export-modal {
  width: 334px;
  padding: 18px 20px 20px;
  border-radius: 10px;
  background: #ffffff;
  color: #172b4d;
  box-shadow: 0 8px 24px rgba(9, 30, 66, 0.28);
}

.export-modal h2 {
  margin: 0 0 12px;
  font-size: 18px;
  font-weight: 800;
}

.export-modal p {
  margin: 0 0 14px;
  color: #172b4d;
  font-size: 13px;
  line-height: 1.45;
}

.export-group {
  display: grid;
  gap: 8px;
  margin: 14px 0;
}

.export-group label {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #172b4d;
  font-size: 13px;
}

.export-label {
  color: #44546f;
  font-size: 12px;
  font-weight: 700;
}

.modal-actions {
  justify-content: flex-end;
  margin-top: 16px;
}

:deep(.el-input__wrapper),
:deep(.el-select__wrapper),
:deep(.el-textarea__inner) {
  border-radius: 8px !important;
}

:deep(.invite-panel .el-input__wrapper),
:deep(.invite-panel .el-textarea__inner),
:deep(.invite-panel .el-select__wrapper) {
  background-color: #ffffff !important;
  box-shadow: 0 0 0 1px #8590a2 inset !important;
}

:deep(.invite-panel .el-input__inner),
:deep(.invite-panel .el-textarea__inner),
:deep(.invite-panel .el-select__placeholder),
:deep(.invite-panel .el-select__selected-item) {
  color: #172b4d !important;
}

@media (max-width: 980px) {
  .page-header,
  .invite-footer {
    flex-direction: column;
    align-items: stretch;
  }

  .page-actions {
    flex-wrap: wrap;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 760px) {
  .content-frame {
    padding: 16px;
  }

  .table-search {
    width: 100%;
  }

  .users-table-shell {
    overflow-x: auto;
  }

  .users-table {
    min-width: 760px;
  }

  .access-header,
  .access-row {
    grid-template-columns: 1fr;
    gap: 10px;
    padding: 12px;
  }

  .access-header {
    display: none;
  }

  .project-grid {
    grid-template-columns: 1fr;
  }

  .close-invite {
    top: 14px;
    left: 14px;
  }

  .invite-shell {
    width: min(100% - 28px, 920px);
    margin-top: 76px;
  }
}
</style>

<style>
/* Unscoped style explicitly for Element Plus dropdowns appended to body */
.admin-project-dropdown {
  background-color: var(--bg-layout, #1a1d24) !important;
  border-color: var(--border-color, #2e3643) !important;
}

.admin-project-dropdown .el-select-dropdown__item {
  color: var(--text-primary, #e4e6ea) !important;
}

.admin-project-dropdown .el-select-dropdown__item.is-hovering,
.admin-project-dropdown .el-select-dropdown__item:hover {
  background-color: var(--bg-hover, #2c333e) !important;
}

.admin-project-dropdown .el-select-dropdown__item.selected {
  color: #3b82f6 !important;
  font-weight: 600;
}
</style>
