const fs = require('fs');
let c = fs.readFileSync('Frontend/src/views/HomeSite/Goals/GoalsDashboard.vue', 'utf8');

const correctBlock = `onMounted(async () => {
  await goalStore.fetchGoals()
  await starredStore.fetchStarredItems()
  await followerStore.fetchFollowedItems()
  await peopleStore.fetchPeople()
  window.addEventListener('global-create-click', openCreateModal)
})

onUnmounted(() => {
  window.removeEventListener('global-create-click', openCreateModal)
})

const isLoading = computed(() => goalStore.isLoading)

const filteredGoals = computed(() => {
  let list = goalStore.goals || []

  // Lọc theo tab
  if (currentTab.value === 'archived') {
    list = list.filter(g => g.isArchived)
  } else if (currentTab.value === 'following') {
    list = list.filter(g => !g.isArchived && g.isFollowing)
  } else {
    // Tất cả mục tiêu (all) thì chỉ hiện những cái chưa archived
    list = list.filter(g => !g.isArchived)
  }

  // Tìm kiếm
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(g => 
      g.title.toLowerCase().includes(q) || 
      (g.status && g.status.toLowerCase().includes(q)) ||
      (g.owner && g.owner.toLowerCase().includes(q))
    )
  }

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
  }

  return list
})

const getStatusClass = (status) => {`;

c = c.replace(/onMounted\(async \(\) => \{[\s\S]*?const getStatusClass = \(status\) => \{/, correctBlock);
fs.writeFileSync('Frontend/src/views/HomeSite/Goals/GoalsDashboard.vue', c);
console.log('Fixed GoalsDashboard.vue');
