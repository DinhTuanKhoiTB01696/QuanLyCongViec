const fs = require('fs');

function updateFile() {
    const file = 'Frontend/src/views/HomeSite/Projects/ProjectDetail.vue';
    let content = fs.readFileSync(file, 'utf-8');

    // Fix description (bio) rendering
    content = content.replace(/<p v-if="project\.description">{{ project\.description }}<\/p>/g, '<div v-if="project.description" v-html="project.description" class="prose"></div>');
    content = content.replace(/<p v-if="project\.reason">{{ project\.reason }}<\/p>/g, '<div v-if="project.reason" v-html="project.reason" class="prose"></div>');
    content = content.replace(/<p v-if="project\.success">{{ project\.success }}<\/p>/g, '<div v-if="project.success" v-html="project.success" class="prose"></div>');

    // Fix avatar color matching by adding email to projectOwner
    content = content.replace(/avatarColor: project\.value\.ownerColor\s*\}\)\)/, 
      'avatarColor: project.value.ownerColor,\n  email: project.value.ownerEmail || project.value.leadEmail || project.value.creatorEmail || project.value.ownerId\n}))');

    // Fix update avatar emails
    content = content.replace(/:user="\{\s*id: update\.creatorId,\s*fullName: update\.creatorName \|\| update\.authorName \|\| project\.owner,\s*avatarUrl: update\.creatorAvatarUrl\s*\}"/g,
      ':user="{ id: update.creatorId, fullName: update.creatorName || update.authorName || project.owner, avatarUrl: update.creatorAvatarUrl, email: update.creatorEmail || update.authorEmail || update.creatorId }"');

    // Fix the status change logic
    const newStatusHtml = `
                  <template v-if="getPreviousStatus(update) === getCurrentStatus(update)">
                    Đã giữ nguyên trạng thái <span class="status-badge mx-1" :class="getStatusClass(getCurrentStatus(update))">{{ getCurrentStatus(update) }}</span>
                  </template>
                  <template v-else>
                    Đã thay đổi trạng thái <span class="status-badge mx-1" :class="getStatusClass(getPreviousStatus(update))">{{ getPreviousStatus(update) }}</span> <i class="fa-solid fa-arrow-right mx-1"></i> <span class="status-badge mx-1" :class="getStatusClass(getCurrentStatus(update))">{{ getCurrentStatus(update) }}</span>
                  </template>
`;

    content = content.replace(/Đã thay đổi trạng thái <span class="status-badge mx-1" :class="getStatusClass\(getPreviousStatus\(update\)\)">\{\{ getPreviousStatus\(update\) \}\}<\/span> <i class="fa-solid fa-arrow-right mx-1"><\/i> <span class="status-badge mx-1" :class="getStatusClass\(getCurrentStatus\(update\)\)">\{\{ getCurrentStatus\(update\) \}\}<\/span>/g, newStatusHtml.trim());
    content = content.replace(/Đ? thay Đ?i tr?ng thi <span class="status-badge mx-1" :class="getStatusClass\(getPreviousStatus\(update\)\)">\{\{ getPreviousStatus\(update\) \}\}<\/span> <i class="fa-solid fa-arrow-right mx-1"><\/i> <span class="status-badge mx-1" :class="getStatusClass\(getCurrentStatus\(update\)\)">\{\{ getCurrentStatus\(update\) \}\}<\/span>/g, newStatusHtml.trim());

    fs.writeFileSync(file, content);
    console.log('ProjectDetail.vue updated');
}

updateFile();
