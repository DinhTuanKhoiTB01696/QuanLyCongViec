const fs = require('fs');

// 1. Fix GoalsDashboard.vue
let goalsFile = 'Frontend/src/views/HomeSite/Goals/GoalsDashboard.vue';
let goalsContent = fs.readFileSync(goalsFile, 'utf8');

// Remove the old filters: "Trạng thái", "Chủ sở hữu", "Nhãn"
// In the current file they might be inside <div class="filter-actions">
// Wait, I already removed them partially or not at all?
// Let's remove the remaining `<button class="filter-btn">` that match Trạng thái, Chủ sở hữu, Nhãn
goalsContent = goalsContent.replace(/<button class="filter-btn">Trạng thái <i[^>]*><\/i><\/button>/g, '');
goalsContent = goalsContent.replace(/<button class="filter-btn">Chủ sở hữu <i[^>]*><\/i><\/button>/g, '');
goalsContent = goalsContent.replace(/<button class="filter-btn">Nhãn <i[^>]*><\/i><\/button>/g, '');

// Remove the filter-actions container if it's empty (just whitespace and Đang theo dõi)
// Actually "Đang theo dõi" is also a filter-btn but it has v-if="currentTab === 'following'"
// Let's leave "Đang theo dõi" but clean up the extra </div> if any.

// Fix filteredGoals computed property to include the dropdown filters correctly
// We need to move the dropdown filter logic INTO the computed property.
goalsContent = goalsContent.replace(/const filteredGoals = computed\(\(\) => \{[\s\S]*?return list\n\s*\}\)[\s\S]*?if \(filters\.value\.status\) \{/m, function(match) {
    // This regex looks for the END of the computed property and the wrongly injected filters.
    return match; // Wait, let's just rewrite the whole computed property robustly.
});

// A better approach: find the wrongly placed filters block and move it.
let badFiltersStr = `
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

if (goalsContent.includes('if (filters.value.status) {')) {
    // It exists! Let's remove it from where it is.
    // It's after `return list\n  })\n`
    goalsContent = goalsContent.replace(/\n\s*if \(filters\.value\.status\) \{[\s\S]*?list = list\.filter\(g => !!g\.isFollowing === isFol\)\n\s*\}/, '');
}

// Now insert it BEFORE `return list` inside the computed property.
if (!goalsContent.includes('list.filter(g => g.status === filters.value.status)')) {
    goalsContent = goalsContent.replace(/return list\n\s*\}\)/, badFiltersStr + '\n\n    return list\n  })');
}

// Also remove the "eye" icon from the "Theo dõi" column
goalsContent = goalsContent.replace(/<i class="fa-solid fa-eye"><\/i> \{\{ goal\.isFollowing \? 'Đang theo dõi' : 'Theo dõi' \}\}/g, "{{ goal.isFollowing ? 'Đang theo dõi' : 'Theo dõi' }}");

fs.writeFileSync(goalsFile, goalsContent);
console.log('Fixed GoalsDashboard.vue');

// 2. Fix ProjectList.vue
let projFile = 'Frontend/src/views/HomeSite/Projects/ProjectList.vue';
let projContent = fs.readFileSync(projFile, 'utf8');

// Remove the old filters: "# Lọc theo Thẻ", etc.
projContent = projContent.replace(/<div class="filters-row mt-16"[^>]*>[\s\S]*?<\/div>\s*<\/div>/, '</div>');
// Wait, replacing `</div>` might be dangerous. Let's just remove the buttons.
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-solid fa-hashtag"><\/i> Lọc theo Thẻ<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-solid fa-signal"><\/i> Trạng thái<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-solid fa-bullseye"><\/i> Mục tiêu<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-solid fa-users"><\/i> Nhóm<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-regular fa-user"><\/i> Chủ sở hữu<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-solid fa-user-group"><\/i> Người đóng góp<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-regular fa-eye"><\/i> Đang theo dõi<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-regular fa-star"><\/i> Có gắn sao<\/button>/g, '');
projContent = projContent.replace(/<button class="filter-btn"><i class="fa-solid fa-sitemap"><\/i> Tuyến báo cáo<\/button>/g, '');
projContent = projContent.replace(/<div class="active-filter-chip">[\s\S]*?<\/div>/g, '');

// Fix width of 'Có gắn sao' column. Search for `.col-star` or `Có gắn sao`
projContent = projContent.replace(/<th class="col-star">Có gắn sao<\/th>/g, '<th class="col-star" style="width: 120px;">Có gắn sao</th>');

// Fix filteredProjects computed property. The previous injection failed because it matched `filtered = filtered` but it should be `list = list`.
let badProjFiltersStr = `
  if (filters.value.status) {
    list = list.filter(p => p.status === filters.value.status)
  }
  if (filters.value.owner) {
    list = list.filter(p => p.owner === filters.value.owner)
  }
  if (filters.value.following) {
    const isFol = filters.value.following === 'true'
    list = list.filter(p => !!p.isFollowing === isFol)
  }
  if (filters.value.starred) {
    const isStar = filters.value.starred === 'true'
    list = list.filter(p => !!p.isStarred === isStar)
  }`;

if (!projContent.includes('list.filter(p => p.status === filters.value.status)')) {
    projContent = projContent.replace(/return list\.map\(p => \(\{/g, badProjFiltersStr + '\n\n    return list.map(p => ({');
}

fs.writeFileSync(projFile, projContent);
console.log('Fixed ProjectList.vue');

// 3. Fix TeamList.vue
let teamListFile = 'Frontend/src/views/HomeSite/Teams/TeamList.vue';
let teamListContent = fs.readFileSync(teamListFile, 'utf8');

// The user wants columns: Đội ngũ, Đội ngũ gốc, Loại đội ngũ, Người quản lý, Thành viên, Đội ngũ con
teamListContent = teamListContent.replace(/<th class="col-members">Thành viên<\/th>/, 
    `<th class="col-parent">Đội ngũ gốc</th>
          <th class="col-type">Loại đội ngũ</th>
          <th class="col-manager">Người quản lý</th>
          <th class="col-members">Thành viên</th>`);

teamListContent = teamListContent.replace(/<td>\{\{ team\.memberCount \}\}<\/td>/,
    `<td>{{ team.parentTeamName }}</td>
          <td>{{ team.type }}</td>
          <td>
            <div style="display: flex; align-items: center; gap: 8px;">
              <UserAvatar v-if="team.manager" :user="{ fullName: team.managerName, email: team.managerEmail }" :size="24" :fontSize="10" />
              <span>{{ team.managerName }}</span>
            </div>
          </td>
          <td>{{ team.memberCount }}</td>`);

// Fix logic in filteredTeams
teamListContent = teamListContent.replace(/avatarText:.*?\,/, (match) => {
    return match + `
    memberCount: t.members?.length || t.users?.length || 0,
    childrenCount: t.children?.length || t.subDepartments?.length || 0,
    manager: t.manager || t.managerId,
    managerName: t.manager?.fullName || t.manager?.name || 'Chưa chọn người quản lý',
    managerEmail: t.manager?.email || '',
    parentTeamName: t.parentDepartment?.name || t.parent?.name || 'Không có đội ngũ gốc',
    type: t.type || 'Đội ngũ chính thức',`;
});

// Remove old memberCount line if any
teamListContent = teamListContent.replace(/memberCount: t\.members\?\.length \|\| 0,\n\s*childrenCount: t\.children\?\.length \|\| 0/, '');

// Need UserAvatar import
if (!teamListContent.includes('UserAvatar.vue')) {
    teamListContent = teamListContent.replace("import { useTeamStore } from '@/store/useTeamStore'", 
        "import { useTeamStore } from '@/store/useTeamStore'\nimport UserAvatar from '@/components/common/UserAvatar.vue'");
}

fs.writeFileSync(teamListFile, teamListContent);
console.log('Fixed TeamList.vue');

// 4. Fix TeamsDashboard.vue (Dành cho bạn)
// Need to add grid/list toggle functionality and Table view.
let teamsDashboardFile = 'Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue';
let teamsDashboardContent = fs.readFileSync(teamsDashboardFile, 'utf8');

// Add viewMode ref if missing
if (!teamsDashboardContent.includes("const viewMode = ref('grid')")) {
    teamsDashboardContent = teamsDashboardContent.replace("const memberInputRef = ref(null)", "const memberInputRef = ref(null)\nconst viewMode = ref('grid')");
}

// Link view-toggle buttons to viewMode
teamsDashboardContent = teamsDashboardContent.replace(/<button class="icon-btn" style="[^"]*"><i class="fa-solid fa-table-cells-large"><\/i><\/button>/, 
    `<button class="icon-btn toggle-btn" :class="{ active: viewMode === 'grid' }" @click="viewMode = 'grid'"><i class="fa-solid fa-table-cells-large"></i></button>`);
teamsDashboardContent = teamsDashboardContent.replace(/<button class="icon-btn" style="[^"]*"><i class="fa-solid fa-list"><\/i><\/button>/, 
    `<button class="icon-btn toggle-btn" :class="{ active: viewMode === 'table' }" @click="viewMode = 'table'"><i class="fa-solid fa-list"></i></button>`);

// Hide team-cards-grid if viewMode !== 'grid'
teamsDashboardContent = teamsDashboardContent.replace(/<div class="team-cards-grid">/, `<div class="team-cards-grid" v-if="viewMode === 'grid'">`);

// Inject Table view after team-cards-grid
let tableHtml = `
          <!-- Table View -->
          <table v-if="viewMode === 'table'" class="jira-table" style="width: 100%; border-collapse: collapse; text-align: left; margin-top: 16px;">
            <thead>
              <tr>
                <th style="padding: 8px 12px; font-size: 12px; color: #5E6C84; border-bottom: 2px solid #DFE1E6;">Đội ngũ</th>
                <th style="padding: 8px 12px; font-size: 12px; color: #5E6C84; border-bottom: 2px solid #DFE1E6;">Đội ngũ gốc</th>
                <th style="padding: 8px 12px; font-size: 12px; color: #5E6C84; border-bottom: 2px solid #DFE1E6;">Loại đội ngũ</th>
                <th style="padding: 8px 12px; font-size: 12px; color: #5E6C84; border-bottom: 2px solid #DFE1E6;">Người quản lý</th>
                <th style="padding: 8px 12px; font-size: 12px; color: #5E6C84; border-bottom: 2px solid #DFE1E6;">Thành viên</th>
                <th style="padding: 8px 12px; font-size: 12px; color: #5E6C84; border-bottom: 2px solid #DFE1E6;">Đội ngũ con</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="team in teams" :key="team.id" @click="goToTeam(team.id)" style="cursor: pointer;">
                <td style="padding: 12px; border-bottom: 1px solid #DFE1E6;">
                  <div style="display: flex; align-items: center; gap: 12px;">
                    <div style="width: 24px; height: 24px; background-color: #00875A; color: white; border-radius: 3px; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: bold;">{{ team.avatarText }}</div>
                    <span style="font-weight: 500; color: #0052CC;">{{ team.name }}</span>
                  </div>
                </td>
                <td style="padding: 12px; border-bottom: 1px solid #DFE1E6;">{{ team.parentTeamName }}</td>
                <td style="padding: 12px; border-bottom: 1px solid #DFE1E6;">{{ team.type }}</td>
                <td style="padding: 12px; border-bottom: 1px solid #DFE1E6;">
                  <div style="display: flex; align-items: center; gap: 8px;">
                    <UserAvatar v-if="team.manager" :user="{ fullName: team.managerName, email: team.managerEmail }" :size="24" :fontSize="10" />
                    <span>{{ team.managerName }}</span>
                  </div>
                </td>
                <td style="padding: 12px; border-bottom: 1px solid #DFE1E6;">{{ team.memberCount }}</td>
                <td style="padding: 12px; border-bottom: 1px solid #DFE1E6;">{{ team.childrenCount }}</td>
              </tr>
            </tbody>
          </table>
`;
teamsDashboardContent = teamsDashboardContent.replace(/<\/div>\n\s*<\/section>/, `</div>\n${tableHtml}\n        </section>`);

// Fix computed `teams` mapping
teamsDashboardContent = teamsDashboardContent.replace(/memberCount: t\.memberCount \?\? t\.members\?\.length \?\? 0,/, 
    `memberCount: t.members?.length || t.users?.length || 0,
  childrenCount: t.children?.length || t.subDepartments?.length || 0,
  manager: t.manager || t.managerId,
  managerName: t.manager?.fullName || t.manager?.name || 'Chưa chọn người quản lý',
  managerEmail: t.manager?.email || '',
  parentTeamName: t.parentDepartment?.name || t.parent?.name || 'Không có đội ngũ gốc',
  type: t.type || 'Đội ngũ chính thức',`);

fs.writeFileSync(teamsDashboardFile, teamsDashboardContent);
console.log('Fixed TeamsDashboard.vue');

