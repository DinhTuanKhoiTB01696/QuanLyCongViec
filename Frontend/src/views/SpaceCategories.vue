<template>
  <AdminLayout>
    <div class="space-categories-page">
      <div class="page-header">
        <h1 class="admin-title">{{ t('Spaces administration', 'Spaces administration') }}</h1>
        <h2 class="admin-subtitle">{{ t('View Space Categories', 'View Space Categories') }}</h2>
        <p class="admin-description">
          {{ t('The table below shows the space categories usable to categorise spaces.', 'The table below shows the space categories usable to categorise spaces.') }}
        </p>
      </div>

      <!-- Categories Table -->
      <div class="table-container">
        <table class="jira-table space-categories-table">
          <thead>
            <tr>
              <th>{{ t('Name', 'Name') }}</th>
              <th>{{ t('Description', 'Description') }}</th>
              <th>{{ t('Spaces', 'Spaces') }}</th>
              <th style="width: 100px; text-align: right;">{{ t('Actions', 'Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="cat in categories" :key="cat.id">
              <td class="cat-name"><strong>{{ cat.name }}</strong></td>
              <td class="cat-desc">{{ cat.description || '-' }}</td>
              <td class="cat-count">
                <span class="count-badge">{{ cat.spacesCount || 0 }}</span>
              </td>
              <td style="text-align: right;">
                <button type="button" class="action-link-danger" @click="deleteCategory(cat.id)">
                  {{ t('Delete', 'Delete') }}
                </button>
              </td>
            </tr>
            <tr v-if="categories.length === 0">
              <td colspan="4" class="empty-state">
                {{ t('No space categories found.', 'No space categories found.') }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Add New Category Form -->
      <div class="add-category-section">
        <h3 class="section-title">{{ t('Add New Space Category', 'Add New Space Category') }}</h3>

        <form @submit.prevent="addCategory" class="jira-form">
          <div class="form-group">
            <label for="cat-name" class="form-label">{{ t('Name', 'Name') }}</label>
            <input
              v-model="newCategory.name"
              type="text"
              id="cat-name"
              required
              placeholder="e.g., Development, Marketing"
              class="jira-input"
            />
          </div>

          <div class="form-group">
            <label for="cat-desc" class="form-label">{{ t('Description', 'Description') }}</label>
            <input
              v-model="newCategory.description"
              type="text"
              id="cat-desc"
              placeholder="Provide a brief description..."
              class="jira-input"
            />
          </div>

          <div class="form-actions">
            <button type="submit" class="jira-btn-primary">
              {{ t('Add', 'Add') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const STORAGE_KEY = 'jira-space-categories'

const categories = ref([])
const newCategory = ref({
  name: '',
  description: ''
})

const defaultCategories = [
  { id: 1, name: 'Development', description: 'Software engineering, tasks, and codes', spacesCount: 1 },
  { id: 2, name: 'Marketing', description: 'Campaigns, designs, and content plans', spacesCount: 0 },
  { id: 3, name: 'Operations', description: 'Support, HR, and facility tasks', spacesCount: 0 }
]

const loadCategories = () => {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (raw) {
    try {
      categories.value = JSON.parse(raw)
    } catch {
      categories.value = defaultCategories
    }
  } else {
    categories.value = defaultCategories
    saveToStorage()
  }
}

const saveToStorage = () => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(categories.value))
}

const addCategory = () => {
  if (!newCategory.value.name.trim()) return

  const exists = categories.value.some(
    c => c.name.toLowerCase() === newCategory.value.name.trim().toLowerCase()
  )
  if (exists) {
    ElMessage.warning(t('Category name already exists.', 'Category name already exists.'))
    return
  }

  const category = {
    id: Date.now(),
    name: newCategory.value.name.trim(),
    description: newCategory.value.description.trim(),
    spacesCount: 0
  }

  categories.value.push(category)
  saveToStorage()

  newCategory.value.name = ''
  newCategory.value.description = ''
  ElMessage.success(t('Space category added successfully.', 'Space category added successfully.'))
}

const deleteCategory = async (id) => {
  try {
    await ElMessageBox.confirm(
      t('Are you sure you want to delete this space category?', 'Are you sure you want to delete this space category?'),
      t('Confirm Delete', 'Confirm Delete'),
      {
        confirmButtonText: t('Delete', 'Delete'),
        cancelButtonText: t('Cancel', 'Cancel'),
        type: 'warning'
      }
    )
    categories.value = categories.value.filter(c => c.id !== id)
    saveToStorage()
    ElMessage.success(t('Space category deleted.', 'Space category deleted.'))
  } catch {
    // Cancelled
  }
}

onMounted(loadCategories)
</script>

<style scoped>
.space-categories-page {
  color: var(--color-text-primary);
  font-family: 'Inter', -apple-system, sans-serif;
  max-width: 800px;
}

.page-header {
  margin-bottom: 24px;
}

.admin-title {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-muted);
  margin: 0 0 6px 0;
}

.admin-subtitle {
  font-size: 20px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 12px 0;
}

.admin-description {
  font-size: 13.5px;
  color: var(--color-text-secondary);
  line-height: 1.5;
  margin: 0;
}

.table-container {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-surface);
  overflow: hidden;
  margin-bottom: 32px;
}

.space-categories-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.space-categories-table th,
.space-categories-table td {
  padding: 12px 16px;
  font-size: 13px;
  border-bottom: 1px solid var(--color-border);
}

.space-categories-table th {
  color: var(--color-text-muted);
  font-weight: 600;
  background: var(--color-bg);
}

.space-categories-table tr:last-child td {
  border-bottom: none;
}

.cat-name {
  color: var(--color-text-primary);
}

.cat-desc {
  color: var(--color-text-secondary);
}

.count-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 20px;
  height: 20px;
  padding: 0 6px;
  border-radius: 99px;
  background: var(--color-border);
  font-size: 11px;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.action-link-danger {
  background: transparent;
  border: none;
  color: #ef4444;
  font-size: 12.5px;
  cursor: pointer;
  padding: 0;
}

.action-link-danger:hover {
  text-decoration: underline;
}

.empty-state {
  text-align: center;
  color: var(--color-text-muted);
  padding: 24px !important;
}

/* Form Styles */
.add-category-section {
  border-top: 1px solid var(--color-border);
  padding-top: 24px;
}

.section-title {
  font-size: 16px;
  font-weight: 600;
  margin: 0 0 16px 0;
  color: var(--color-text-primary);
}

.jira-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-width: 480px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.jira-input {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 13.5px;
  outline: none;
  width: 100%;
  box-sizing: border-box;
}

.jira-input:focus {
  border-color: var(--color-accent);
}

.jira-btn-primary {
  background: var(--color-accent);
  color: var(--color-text-inverse);
  border: none;
  border-radius: 6px;
  padding: 8px 16px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s;
}

.jira-btn-primary:hover {
  opacity: 0.9;
}
</style>
