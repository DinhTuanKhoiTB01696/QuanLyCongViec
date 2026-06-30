const fs = require('fs');

function updateFile(file) {
    let content = fs.readFileSync(file, 'utf-8');

    // Remove old header and add 'Quay lại' on the right side of the content wrapper
    const oldHeaderRegex = /<!-- Header -->\s*<div style="padding: 16px 24px; display: flex; justify-content: space-between; align-items: center;">[\s\S]*?<\/div>\s*<!-- Content -->/;
    if (oldHeaderRegex.test(content)) {
        content = content.replace(oldHeaderRegex, '<!-- Content -->');
    }

    // Add Quay lại inside the 600px wrapper
    const wrapperRegex = /<div style="width: 100%; max-width: 600px; display: flex; flex-direction: column; gap: 24px; position: relative;">/;
    if (wrapperRegex.test(content) && !content.includes('fa-arrow-right')) {
        content = content.replace(wrapperRegex, `<div style="width: 100%; max-width: 600px; display: flex; flex-direction: column; gap: 24px; position: relative;">
             <!-- Top Actions -->
             <div style="display: flex; justify-content: flex-end; margin-bottom: -16px;">
                <button class="icon-btn" @click="isGiveKudosOpen = false" style="background: transparent; border: none; font-size: 14px; font-weight: 500; cursor: pointer; color: #42526E; display: flex; align-items: center; gap: 8px; padding: 4px 8px; border-radius: 4px; transition: background 0.1s;" onmouseover="this.style.background='#EBECF0'" onmouseout="this.style.background='transparent'">Quay lại <i class="fa-solid fa-arrow-right"></i></button>
             </div>`);
    }

    // Fix cá nhân hóa color
    content = content.replace(/<button class="secondary-btn" style="position: absolute; top: 12px; right: 12px; font-size: 12px; padding: 4px 8px; height: auto; pointer-events: none;">Cá nhân hóa<\/button>/g, 
        `<button class="secondary-btn" style="position: absolute; top: 12px; right: 12px; font-size: 12px; padding: 4px 8px; height: auto; pointer-events: none; color: white; background: rgba(255,255,255,0.2); border: none;">Cá nhân hóa</button>`);

    // Fix target dropdown to use UserAvatar
    const targetAvatarRegex = /<div class="member-avatar-micro" :style="\{ backgroundColor: kudosTargetType === 'team' \? '#E2B203' : '#0052CC', color: 'white' \}">(.*?)<\/div>/;
    if (targetAvatarRegex.test(content)) {
        content = content.replace(targetAvatarRegex, `<UserAvatar v-if="kudosTargetType === 'user'" :user="kudosTargetData || {}" :size="24" :fontSize="10" />
                     <div v-else class="member-avatar-micro" style="background-color: #36B37E; color: white; width: 24px; height: 24px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px;">$1</div>`);
    }

    fs.writeFileSync(file, content);
    console.log('Updated ' + file);
}

updateFile('Frontend/src/views/HomeSite/Teams/TeamDetail.vue');
updateFile('Frontend/src/views/HomeSite/Teams/TeamKudos.vue');
