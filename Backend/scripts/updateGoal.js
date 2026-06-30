const fs = require('fs');

function updateFile() {
    const file = 'Frontend/src/views/HomeSite/Goals/GoalDetail.vue';
    let content = fs.readFileSync(file, 'utf-8');

    // 1. Add Tuần này visual
    const visualHtml = `
          <div class="timeline-visual">
            <div class="timeline-line"></div>
            <div class="timeline-node current">
              <i class="fa-solid fa-user-group"></i>
              <span>Tuần này</span>
            </div>
          </div>
`;
    // Insert before update-editor-box if not already there
    if (!content.includes('timeline-visual')) {
        content = content.replace(/<div class="update-editor-box">/, visualHtml + '\n          <div class="update-editor-box">');
    }

    // 2. Fix empty state
    const oldEmptyStateRegex = /<div class="timeline-posts" v-else>\s*<div class="empty-state text-center py-10" style="color: #6B778C;">[\s\S]*?<\/div>\s*<\/div>/;
    const newEmptyState = `          <div v-else class="empty-state-large-tab">
            <div class="empty-illustration">
              <i class="fa-regular fa-message" style="color: #0052CC; font-size: 56px;"></i>
            </div>
            <div class="empty-content">
              <h3>Chưa có bản cập nhật nào.</h3>
              <p>Các bản cập nhật mục tiêu sẽ xuất hiện ở đây sau khi được đăng.</p>
            </div>
          </div>`;
    content = content.replace(oldEmptyStateRegex, newEmptyState);

    // 3. Avatar email fix in Overview tab owner chip
    content = content.replace(/<UserAvatar :user="\{ id: goal\.creatorId \|\| goal\.ownerId, avatarColor: goal\.creatorColor \|\| goal\.ownerColor, fullName: goal\.creatorName \|\| goal\.ownerName \|\| goal\.owner, avatarUrl: goal\.creatorAvatarUrl \|\| goal\.ownerAvatarUrl \}"/g,
      '<UserAvatar :user="{ id: goal.creatorId || goal.ownerId, avatarColor: goal.creatorColor || goal.ownerColor, fullName: goal.creatorName || goal.ownerName || goal.owner, avatarUrl: goal.creatorAvatarUrl || goal.ownerAvatarUrl, email: goal.creatorEmail || goal.ownerEmail }"');

    // 4. Avatar email fix in Updates tab (updates list)
    content = content.replace(/<UserAvatar :user="\{ id: update\.creatorId, fullName: update\.creatorName, avatarUrl: update\.creatorAvatarUrl \}"/g,
      '<UserAvatar :user="{ id: update.creatorId, fullName: update.creatorName, avatarUrl: update.creatorAvatarUrl, email: update.creatorEmail }"');

    // 5. Also fix lesson, risk, decision avatars
    content = content.replace(/<UserAvatar :user="\{ id: item\.creatorId, email: item\.creatorEmail, fullName: item\.creatorName, avatarUrl: item\.creatorAvatarUrl \}"/g,
      '<UserAvatar :user="{ id: item.creatorId, fullName: item.creatorName, avatarUrl: item.creatorAvatarUrl, email: item.creatorEmail }"');

    // 6. Fix "email: item.creatorEmail" already present cases just to be sure it matches the property
    // Wait, the regex in #5 catches it. Let's make it more general:
    content = content.replace(/:user="\{\s*id: [a-zA-Z\.]+(creatorId|ownerId)[^\}]+\}"/g, (match) => {
        if (!match.includes('email:')) {
           // It's probably easier to just do it manually for the exact matches:
           return match.replace('}', ', email: update.creatorEmail || update.ownerEmail }');
        }
        return match;
    });

    // Let's just do a blanket replace for update avatars if not handled
    content = content.replace(/<UserAvatar :user="\{ id: update\.creatorId, fullName: update\.creatorName, avatarUrl: update\.creatorAvatarUrl \}"/g,
      '<UserAvatar :user="{ id: update.creatorId, fullName: update.creatorName, avatarUrl: update.creatorAvatarUrl, email: update.creatorEmail }"');

    fs.writeFileSync(file, content);
    console.log('GoalDetail.vue updated');
}

updateFile();
