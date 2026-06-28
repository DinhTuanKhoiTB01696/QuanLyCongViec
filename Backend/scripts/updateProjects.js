const fs = require('fs');

function updateProjectList() {
    const file = 'Frontend/src/views/HomeSite/Projects/ProjectList.vue';
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
  owner: '',
  following: '',
  starred: ''
})

const uniqueValues = (selector) => Array.from(new Set(
  (projectStore.projects || [])
    .map(selector)
    .filter(value => value && value !== 'N/A')
)).sort()

const statusOptions = computed(() => uniqueValues(p => p.status))
const ownerOptions = computed(() => uniqueValues(p => p.owner))

const booleanOptions = [
  { label: 'Có', value: 'true' },
  { label: 'Không', value: 'false' }
]

const clearFilters = () => {
  filters.value = {
    status: '',
    owner: '',
    following: '',
    starred: ''
  }
}
const hasActiveFilters = computed(() => Object.values(filters.value).some(val => val !== ''))
`);
    }

    // Update filteredProjects logic
    content = content.replace(/if \(searchQuery\.value\) \{[\s\S]*?filtered = filtered\.filter\(p =>[\s\S]*?\}\)/, (match) => {
        return match + `\n
  if (filters.value.status) {
    filtered = filtered.filter(p => p.status === filters.value.status)
  }
  if (filters.value.owner) {
    filtered = filtered.filter(p => p.owner === filters.value.owner)
  }
  if (filters.value.following) {
    const isFol = filters.value.following === 'true'
    filtered = filtered.filter(p => !!p.isFollowing === isFol)
  }
  if (filters.value.starred) {
    const isStar = filters.value.starred === 'true'
    filtered = filtered.filter(p => !!p.isStarred === isStar)
  }`;
    });

    // Inject filter chips HTML
    const htmlToInject = `
        <div class="filter-chips" style="display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; width: 100%;">
          <DropdownFilter label="Trạng thái" :options="statusOptions" v-model="filters.status" />
          <DropdownFilter label="Chủ sở hữu" :options="ownerOptions" v-model="filters.owner" />
          <DropdownFilter label="Đang theo dõi" :options="booleanOptions" v-model="filters.following" />
          <DropdownFilter label="Có gắn sao" :options="booleanOptions" v-model="filters.starred" />
          <button v-if="hasActiveFilters" class="clear-filters-btn filter-chip" @click="clearFilters">Xóa lọc</button>
        </div>
`;
    // We want to place this after the search bar container (search-box-wrapper)
    content = content.replace(/<div class="search-box-wrapper"[^>]*>[\s\S]*?<\/div>/, (match) => {
        return match + htmlToInject;
    });

    // Add CSS for clear-filters-btn if not exists
    if (!content.includes('.clear-filters-btn')) {
        content = content.replace(/<\/style>/, `
.clear-filters-btn {
  background: white;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  color: #0052CC;
  cursor: pointer;
  transition: background 0.2s;
}
.clear-filters-btn:hover {
  background: #E6FCFF;
}
</style>`);
    }

    fs.writeFileSync(file, content);
    console.log('ProjectList.vue updated');
}

updateProjectList();
