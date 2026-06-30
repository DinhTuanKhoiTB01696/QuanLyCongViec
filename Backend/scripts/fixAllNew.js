const fs = require('fs');

// --- 1. Fix GoalsDashboard.vue ---
let goalsPath = 'Frontend/src/views/HomeSite/Goals/GoalsDashboard.vue';
let goalsContent = fs.readFileSync(goalsPath, 'utf8');

// Find where the duplication starts
let duplicateMarker = "  } else if (currentTab.value === 'following') {\n    list = list.filter(g => !g.isArchived && g.isFollowing)\n  await followerStore.fetchFollowedItems()\n  await peopleStore.fetchPeople()\n  window.addEventListener('global-create-click', openCreateModal)\n})\n\nonUnmounted(() => {\n  window.removeEventListener('global-create-click', openCreateModal)\n})\n\nconst isLoading = computed(() => goalStore.isLoading)\n\nconst filteredGoals = computed(() => {\n  let list = goalStore.goals || []\n\n  // Lọc theo tab\n  if (currentTab.value === 'archived') {\n    list = list.filter(g => g.isArchived)\n  } else if (currentTab.value === 'following') {\n    list = list.filter(g => !g.isArchived && g.isFollowing)\n  } else {\n    // Tất cả mục tiêu (all) thì chỉ hiện những cái chưa archived\n    list = list.filter(g => !g.isArchived)\n  }";

if (goalsContent.includes(duplicateMarker)) {
    goalsContent = goalsContent.replace(duplicateMarker, `  } else if (currentTab.value === 'following') {
    list = list.filter(g => !g.isArchived && g.isFollowing)
  } else {
    // Tất cả mục tiêu (all) thì chỉ hiện những cái chưa archived
    list = list.filter(g => !g.isArchived)
  }`);
}

// Check if the old trailing `if (filters.value.status)` block is still floating around at the end
if (goalsContent.includes('list.filter(g => !!g.isFollowing === isFol)')) {
    // We already removed it in the last run, but let's be sure.
}

// Make sure `filteredGoals` has the filters.
let filterLogicStr = `  if (filters.value.status) {
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
  }

  return list`;

if (!goalsContent.includes('if (filters.value.status)')) {
    goalsContent = goalsContent.replace(/return list\s*\}\)/, filterLogicStr + '\n})');
}

fs.writeFileSync(goalsPath, goalsContent);
console.log('GoalsDashboard.vue fixed');

// --- 2. Fix ProjectList.vue ---
let projPath = 'Frontend/src/views/HomeSite/Projects/ProjectList.vue';
let projContent = fs.readFileSync(projPath, 'utf8');

// Inject filter-chips into HTML if missing
let projectFiltersHtml = `        <div class="filter-chips" style="display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; width: 100%;">
          <DropdownFilter label="Trạng thái" :options="statusOptions" v-model="filters.status" />
          <DropdownFilter label="Chủ sở hữu" :options="ownerOptions" v-model="filters.owner" />
          <DropdownFilter label="Đang theo dõi" :options="booleanOptions" v-model="filters.following" />
          <DropdownFilter label="Có gắn sao" :options="booleanOptions" v-model="filters.starred" />
          <button v-if="hasActiveFilters" class="clear-filters-btn" @click="clearFilters" style="padding: 6px 12px; border-radius: 16px; border: 1px solid #DFE1E6; background: #fff; cursor: pointer; color: #5E6C84; font-size: 14px;">Xóa lọc</button>
        </div>`;

if (!projContent.includes('<DropdownFilter label="Trạng thái"')) {
    projContent = projContent.replace(/<div class="search-box-full">[\s\S]*?<\/div>\s*<\/div>/, (match) => {
        // match contains: <div class="search-box-full">...</div>\n      </div>
        // we insert right before the closing </div>
        let withoutClosing = match.substring(0, match.lastIndexOf('</div>'));
        return withoutClosing + projectFiltersHtml + '\n      </div>';
    });
}

// Add state for filters to script setup
let projScriptState = `
const statusOptions = computed(() => {
  const statuses = projectStore.projects?.map(p => p.status) || []
  return [...new Set(statuses)].filter(Boolean)
})
const ownerOptions = computed(() => {
  const owners = projectStore.projects?.map(p => p.owner) || []
  return [...new Set(owners)].filter(Boolean)
})
const booleanOptions = ['true', 'false']

const filters = ref({
  status: '',
  owner: '',
  following: '',
  starred: ''
})

const hasActiveFilters = computed(() => {
  return Object.values(filters.value).some(v => v !== '')
})

const clearFilters = () => {
  filters.value = {
    status: '',
    owner: '',
    following: '',
    starred: ''
  }
}
`;

if (!projContent.includes('const filters = ref({')) {
    projContent = projContent.replace(/const isFollowing = computed\(\(\) => currentTab\.value === 'following'\)/, 
        `const isFollowing = computed(() => currentTab.value === 'following')\n` + projScriptState);
}

// Check for missing DropdownFilter import
if (!projContent.includes('DropdownFilter.vue')) {
    projContent = projContent.replace("import { useRouter } from 'vue-router'", 
        "import { useRouter } from 'vue-router'\nimport DropdownFilter from '@/components/common/DropdownFilter.vue'");
}

// Add filter logic to filteredProjects
let projFilterLogicStr = `  if (filters.value.status) {
    list = list.filter(p => p.status === filters.value.status)
  }
  if (filters.value.owner) {
    list = list.filter(p => p.owner === filters.value.owner)
  }
  if (filters.value.following) {
    const isFol = filters.value.following === 'true'
    list = list.filter(p => {
      const isPfol = followerStore.followedItems?.some(i => i.entityId === p.id) || false
      return !!isPfol === isFol
    })
  }
  if (filters.value.starred) {
    const isStar = filters.value.starred === 'true'
    list = list.filter(p => {
      const isPstar = starredStore.starredItems?.some(i => i.itemId === p.id) || false
      return !!isPstar === isStar
    })
  }

  return list.map`;

if (!projContent.includes('filters.value.starred')) {
    projContent = projContent.replace(/return list\.map/, projFilterLogicStr);
}

fs.writeFileSync(projPath, projContent);
console.log('ProjectList.vue fixed');
