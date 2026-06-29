const fs = require('fs');

function updateGoalsDashboard() {
    const file = 'Frontend/src/views/HomeSite/Goals/GoalsDashboard.vue';
    let content = fs.readFileSync(file, 'utf-8');

    // Add import
    if (!content.includes('DropdownFilter.vue')) {
        content = content.replace("import UserAvatar from '@/components/common/UserAvatar.vue'", 
            "import UserAvatar from '@/components/common/UserAvatar.vue'\nimport DropdownFilter from '@/components/common/DropdownFilter.vue'");
    }

    // Add filters ref and computed options
    if (!content.includes('const filters = ref({')) {
        content = content.replace("const searchQuery = ref('')",
`const searchQuery = ref('')
const filters = ref({
  status: '',
  progress: '',
  favorite: '',
  following: '',
  owner: ''
})

const uniqueValues = (selector) => Array.from(new Set(
  (goalStore.goals || [])
    .map(selector)
    .filter(value => value && value !== 'N/A')
)).sort()

const statusOptions = computed(() => uniqueValues(g => g.status))
const ownerOptions = computed(() => uniqueValues(g => g.owner))
const progressOptions = [
  { label: 'Chưa bắt đầu (0%)', value: '0' },
  { label: 'Đang tiến hành (>0%)', value: 'in_progress' },
  { label: 'Hoàn thành (100%)', value: '100' }
]
const booleanOptions = [
  { label: 'Có', value: 'true' },
  { label: 'Không', value: 'false' }
]

const clearFilters = () => {
  filters.value = {
    status: '',
    progress: '',
    favorite: '',
    following: '',
    owner: ''
  }
}
const hasActiveFilters = computed(() => Object.values(filters.value).some(val => val !== ''))
`);
    }

    // Update filteredGoals logic
    content = content.replace(/if \(searchQuery\.value\) \{[\s\S]*?list = list\.filter\(g =>[\s\S]*?\}\)/, (match) => {
        return match + `\n
  if (filters.value.status) {
    list = list.filter(g => g.status === filters.value.status)
  }
  if (filters.value.owner) {
    list = list.filter(g => g.owner === filters.value.owner)
  }
  if (filters.value.progress) {
    list = list.filter(g => {
      if (filters.value.progress === '0') return g.progress === 0
      if (filters.value.progress === '100') return g.progress === 100
      if (filters.value.progress === 'in_progress') return g.progress > 0 && g.progress < 100
      return true
    })
  }
  if (filters.value.favorite) {
    const isFav = filters.value.favorite === 'true'
    list = list.filter(g => !!g.isFavorite === isFav)
  }
  if (filters.value.following) {
    const isFol = filters.value.following === 'true'
    list = list.filter(g => !!g.isFollowing === isFol)
  }`;
    });

    // Inject filter chips HTML
    const htmlToInject = `
          </div>
          <div class="filter-chips" style="display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; width: 100%;">
            <DropdownFilter label="Trạng thái" :options="statusOptions" v-model="filters.status" />
            <DropdownFilter label="Chủ sở hữu" :options="ownerOptions" v-model="filters.owner" />
            <DropdownFilter label="Tiến độ" :options="progressOptions" v-model="filters.progress" />
            <DropdownFilter label="Yêu thích" :options="booleanOptions" v-model="filters.favorite" />
            <DropdownFilter label="Theo dõi" :options="booleanOptions" v-model="filters.following" />
            <button v-if="hasActiveFilters" class="clear-filters-btn" @click="clearFilters">Xóa lọc</button>
          </div>
`;
    // We want to place this after the search bar (filter-actions div)
    content = content.replace(/<div class="filter-actions">[\s\S]*?<\/div>/, (match) => {
        return match + htmlToInject;
    });

    fs.writeFileSync(file, content);
    console.log('GoalsDashboard.vue updated');
}

updateGoalsDashboard();
