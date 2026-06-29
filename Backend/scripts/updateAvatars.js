const fs = require('fs');

// --- 1. USER AVATAR UPDATE ---
function updateUserAvatar() {
    const file = 'Frontend/src/components/common/UserAvatar.vue';
    let content = fs.readFileSync(file, 'utf-8');

    if (!content.includes('usePeopleStore')) {
        content = content.replace("import { getAvatarColor, getInitials } from '@/utils/avatarHelper'",
            "import { getAvatarColor, getInitials } from '@/utils/avatarHelper'\nimport { usePeopleStore } from '@/store/usePeopleStore'");
        
        const scriptStart = content.indexOf('const props = defineProps(');
        content = content.slice(0, scriptStart) + "const peopleStore = usePeopleStore()\n\n" + content.slice(scriptStart);

        content = content.replace("const resolvedEmail = computed(() => props.user?.email || props.user?.Email || '')",
`const resolvedEmail = computed(() => {
  let email = props.user?.email || props.user?.Email || ''
  if (!email && resolvedId.value) {
     const p = peopleStore.users?.find(u => u.id === resolvedId.value)
     if (p?.email) email = p.email
  }
  return email
})`);
        fs.writeFileSync(file, content);
        console.log('UserAvatar.vue updated');
    }
}

// --- 2. GOAL DETAIL UPDATE ---
function updateGoalDetail() {
    const file = 'Frontend/src/views/HomeSite/Goals/GoalDetail.vue';
    let content = fs.readFileSync(file, 'utf-8');

    // Replace hardcoded history avatar
    content = content.replace(
        /<div class="user-avatar-current"[^>]*>{{ getInitials\(entry\.actor\) }}<\/div>/g,
        '<UserAvatar :user="{ fullName: entry.actor, email: entry.email }" :size="24" :fontSize="10" />'
    );

    // Update goalHistory to include email
    content = content.replace(/actor: upd\.creatorName[^,]+,/g, (match) => {
        return match + '\n    email: upd.creatorEmail || upd.authorEmail,';
    });
    content = content.replace(/actor: entry\.actor[^,]+,/g, (match) => {
        return match + '\n      email: entry.creatorEmail || entry.authorEmail,';
    });

    // Fix CSS of .owner-avatar-micro
    const cssMatch = /\.owner-avatar-micro\s*\{[^}]+\}/;
    content = content.replace(cssMatch, `.owner-avatar-micro {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}`);

    fs.writeFileSync(file, content);
    console.log('GoalDetail.vue updated');
}

// --- 3. PROJECT DETAIL UPDATE ---
function updateProjectDetail() {
    const file = 'Frontend/src/views/HomeSite/Projects/ProjectDetail.vue';
    let content = fs.readFileSync(file, 'utf-8');

    // Add margin to timeline-post
    content = content.replace(/<div class="timeline-post" v-for="update in projectUpdates" :key="update\.id">/,
        '<div class="timeline-post" style="margin-bottom: 24px; border: 1px solid #DFE1E6; padding: 16px; border-radius: 3px; background: white;" v-for="update in projectUpdates" :key="update.id">');

    // Add activity tab refs
    if (!content.includes("const activityTab = ref('comments')")) {
        content = content.replace(/const currentTab = ref\('overview'\)/, "const currentTab = ref('overview')\nconst activityTab = ref('comments')");
    }

    // Add projectHistory computed
    if (!content.includes("const projectHistory = computed")) {
        content = content.replace(/const projectUpdates = computed\(\(\) => projectStore\.updates \|\| \[\]\)/,
`const projectUpdates = computed(() => projectStore.updates || [])
const projectHistory = computed(() => {
  const explicitHistory = projectStore.history || project.value?.history || []
  if (explicitHistory.length) {
    return explicitHistory.map((entry, index) => ({
      id: entry.id || index,
      actor: entry.actor || entry.creatorName || project.value?.creatorName || 'Người dùng',
      email: entry.creatorEmail || entry.authorEmail,
      action: entry.action || entry.title || 'đã cập nhật',
      target: entry.target || entry.content || '',
      createdAt: entry.createdAt
    }))
  }
  return projectUpdates.value.map(upd => ({
    id: upd.id,
    actor: upd.creatorName || upd.authorName || project.value?.owner || 'Người dùng',
    email: upd.creatorEmail || upd.authorEmail,
    action: 'đã đăng bản cập nhật',
    target: '',
    createdAt: upd.createdAt || upd.updatedAt
  }))
})`);
    }

    // Replace comment section with Hoạt động block
    const commentSectionOld = /<section class="content-section">\s*<CommentSection :entity-id="route\.params\.id" entity-type="Project" \/>\s*<\/section>/;
    const newHoatDong = `
          <!-- Hoạt động -->
          <section class="content-section" style="margin-top: 16px;">
            <div class="section-header" style="border: none; padding-bottom: 0;">
              <h3>Hoạt động</h3>
            </div>
            <div style="display: flex; gap: 8px; margin-bottom: 16px;">
              <button class="toggle-btn" :class="{ active: activityTab === 'comments' }" @click="activityTab = 'comments'">Nhận xét</button>
              <button class="toggle-btn" :class="{ active: activityTab === 'history' }" @click="activityTab = 'history'">Lịch sử</button>
            </div>
            
            <div class="section-body">
              <div v-if="activityTab === 'comments'">
                <CommentSection :entity-id="route.params.id" entity-type="Project" />
              </div>
              <div v-else>
                <div class="timeline-item" v-for="entry in projectHistory" :key="entry.id" style="display: flex; align-items: flex-start; gap: 12px; margin-bottom: 16px;">
                   <UserAvatar :user="{ fullName: entry.actor, email: entry.email }" :size="24" :fontSize="10" />
                   <div style="flex: 1;">
                      <div style="display: flex; justify-content: space-between; align-items: center;">
                         <span style="font-size: 14px; color: #172B4D;"><strong>{{ entry.actor }}</strong> {{ entry.action }}</span>
                         <span style="font-size: 12px; color: #5E6C84;">{{ formatDate(entry.createdAt) }}</span>
                      </div>
                      <div v-if="entry.target" style="font-size: 14px; color: #5E6C84; margin-top: 4px;">{{ entry.target }}</div>
                   </div>
                </div>
                <div v-if="projectHistory.length === 0" style="color: #6B778C; font-size: 14px; padding: 12px 0;">Chưa có hoạt động nào.</div>
              </div>
            </div>
          </section>`;
    content = content.replace(commentSectionOld, newHoatDong);

    fs.writeFileSync(file, content);
    console.log('ProjectDetail.vue updated');
}

updateUserAvatar();
updateGoalDetail();
updateProjectDetail();
