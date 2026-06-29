const fs = require('fs');

function fixTeamTable(filePath) {
  if (!fs.existsSync(filePath)) return;
  let content = fs.readFileSync(filePath, 'utf8');

  // Fix memberCount to include t.memberCount
  content = content.replace(/memberCount: t\.members\?\.length \|\| t\.users\?\.length \|\| 0/, 
    'memberCount: t.memberCount ?? t.members?.length ?? t.users?.length ?? 0');
  
  content = content.replace(/managerName: t\.manager\?\.fullName \|\| t\.manager\?\.name \|\| 'Chưa chọn người quản lý'/, 
    "managerName: t.manager?.fullName || t.manager?.name || 'Chưa có'");

  // Add parentCount mapping
  if (!content.includes('parentCount:')) {
    content = content.replace(/parentTeamName: t\.parentDepartment\?\.name \|\| t\.parent\?\.name \|\| 'Không có đội ngũ gốc',/,
      "parentTeamName: t.parentDepartment?.name || t.parent?.name || 'Không có đội ngũ gốc',\n    parentCount: (t.parentDepartment || t.parent || t.parentId) ? 1 : 0,");
  }

  // Rearrange columns in HTML <thead>
  // Current:
  // <th style="width: 25%">Đội ngũ</th>
  // <th style="width: 15%">Đội ngũ gốc</th>
  // <th style="width: 15%">Loại đội ngũ</th>
  // <th style="width: 20%">Người quản lý</th>
  // <th style="width: 15%">Thành viên</th>
  // <th style="width: 10%">Đội ngũ con <i class="fa-solid fa-arrow-down" style="font-size: 10px; margin-left: 4px;"></i></th>
  let newThead = `            <thead>
              <tr>
                <th style="width: 30%">Đội ngũ</th>
                <th style="width: 15%">Loại đội ngũ</th>
                <th style="width: 20%">Người quản lý</th>
                <th style="width: 10%">Thành viên</th>
                <th style="width: 10%">Đội ngũ gốc</th>
                <th style="width: 15%">Đội ngũ con <i class="fa-solid fa-arrow-down" style="font-size: 10px; margin-left: 4px;"></i></th>
              </tr>
            </thead>`;
  content = content.replace(/<thead>[\s\S]*?<\/thead>/, newThead);

  // Rearrange columns in HTML <tbody>
  // Current tbody structure:
  // <td> ... </td> (name)
  // <td>{{ team.parentTeamName }}</td>
  // <td>{{ team.type }}</td>
  // <td> ... </td> (manager)
  // <td>{{ team.memberCount }}</td>
  // <td>{{ team.childrenCount }}</td>
  let newTbody = `            <tbody>
              <tr v-for="team in (viewMode === 'table' && filteredTeams ? filteredTeams : teams)" :key="team.id" @click="goToTeam(team.id)">
                <td>
                  <div class="team-name-cell">
                    <div class="team-avatar-small" :style="{ backgroundColor: '#0052cc' }">{{ team.avatarText }}</div>
                    <span class="team-name-text">{{ team.name }}</span>
                  </div>
                </td>
                <td style="white-space: nowrap;">{{ team.type }}</td>
                <td>
                  <div v-if="team.managerName !== 'Chưa có'" class="manager-cell" style="display: flex; align-items: center; gap: 8px;">
                    <UserAvatar :user="{ fullName: team.managerName, email: team.managerEmail }" :size="24" :fontSize="10" />
                    <span style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 120px;">{{ team.managerName }}</span>
                  </div>
                  <div v-else style="color: #5E6C84; display: flex; align-items: center; gap: 6px;">
                    <div style="width: 24px; height: 24px; border-radius: 50%; background: #DFE1E6; display: flex; align-items: center; justify-content: center; color: #172B4D; font-size: 10px; font-weight: bold;">?</div>
                    <span>Chưa có</span>
                  </div>
                </td>
                <td>{{ team.memberCount }}</td>
                <td>{{ team.parentCount }}</td>
                <td>{{ team.childrenCount }}</td>
              </tr>
            </tbody>`;
  content = content.replace(/<tbody>[\s\S]*?<\/tbody>/, newTbody);

  fs.writeFileSync(filePath, content);
}

fixTeamTable('Frontend/src/views/HomeSite/Teams/TeamList.vue');
fixTeamTable('Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue');

console.log('Fixed tables');
