const fs = require('fs');

// --- ProfileDetail.vue ---
let profilePath = 'Frontend/src/views/HomeSite/People/ProfileDetail.vue';
let profileContent = fs.readFileSync(profilePath, 'utf8');

// 1. In ProfileDetail.vue, we need to fix the Bio and Hobbies sections
// Existing bio section:
/*
          <section class="info-section">
            <h3>Bio</h3>
            <div class="bio-content-wrapper" :class="{ 'is-editing': editingBio }">
              <RichTextEditor 
                v-if="editingBio"
                v-model="user.bio"
                @blur="saveBio"
              />
              <div 
                v-else 
                class="bio-display" 
                @click="editingBio = true"
                v-html="user.bio || '<p class=\'text-gray-400\'>Thêm giới thiệu về bạn...</p>'"
                style="min-height: 100px; padding: 12px; border: 1px solid transparent; border-radius: 4px; cursor: pointer;"
                @mouseenter="$event.target.style.borderColor = '#dfe1e6'"
                @mouseleave="$event.target.style.borderColor = 'transparent'"
              ></div>
            </div>
          </section>
*/

let newBioSection = `          <section class="info-section">
            <h3>Bio</h3>
            <div class="bio-content-wrapper" :class="{ 'is-editing': editingBio }">
              <RichTextEditor 
                v-if="editingBio"
                v-model="tempBio"
                @save="saveBio"
                @cancel="editingBio = false"
                placeholder="Thêm giới thiệu về bạn..."
              />
              <div 
                v-else 
                class="bio-display tiptap-content" 
                @click="startEditingBio"
                style="min-height: 40px; padding: 8px; border: 1px solid transparent; border-radius: 4px; cursor: pointer;"
                onmouseover="this.style.backgroundColor='#FAFBFC'"
                onmouseout="this.style.backgroundColor='transparent'"
              >
                <div v-if="user.bio && user.bio !== '<p></p>'" v-html="user.bio"></div>
                <div v-else style="color: #5E6C84;">Thêm giới thiệu về bạn...</div>
              </div>
            </div>
          </section>`;

profileContent = profileContent.replace(/<section class="info-section">\s*<h3>Bio<\/h3>[\s\S]*?<\/section>/, newBioSection);

let newHobbiesSection = `          <section class="info-section">
            <h3>Hobbies & Interests</h3>
            <div class="bio-content-wrapper" :class="{ 'is-editing': editingHobbies }">
              <RichTextEditor 
                v-if="editingHobbies"
                v-model="tempHobbies"
                @save="saveHobbies"
                @cancel="editingHobbies = false"
                placeholder="Chia sẻ sở thích của bạn..."
              />
              <div 
                v-else 
                class="bio-display tiptap-content" 
                @click="startEditingHobbies"
                style="min-height: 40px; padding: 8px; border: 1px solid transparent; border-radius: 4px; cursor: pointer;"
                onmouseover="this.style.backgroundColor='#FAFBFC'"
                onmouseout="this.style.backgroundColor='transparent'"
              >
                <div v-if="user.hobbies && user.hobbies !== '<p></p>'" v-html="user.hobbies"></div>
                <div v-else style="color: #5E6C84;">Has not shared any hobbies yet.</div>
              </div>
            </div>
          </section>`;

profileContent = profileContent.replace(/<section class="info-section">\s*<h3>Hobbies & Interests<\/h3>[\s\S]*?<\/section>/, newHobbiesSection);

// Add the missing state variables and methods to script setup
let scriptAdditions = `
const editingBio = ref(false)
const tempBio = ref('')
const startEditingBio = () => {
  tempBio.value = user.value.bio || ''
  editingBio.value = true
}
const saveBio = async () => {
  try {
    await peopleStore.updateProfile({ bio: tempBio.value })
    editingBio.value = false
    await peopleStore.fetchProfileDetail(route.params.id)
  } catch(e) { console.error('Save bio failed', e) }
}

const editingHobbies = ref(false)
const tempHobbies = ref('')
const startEditingHobbies = () => {
  tempHobbies.value = user.value.hobbies || ''
  editingHobbies.value = true
}
const saveHobbies = async () => {
  try {
    await peopleStore.updateProfile({ hobbies: tempHobbies.value })
    editingHobbies.value = false
    await peopleStore.fetchProfileDetail(route.params.id)
  } catch(e) { console.error('Save hobbies failed', e) }
}
`;

profileContent = profileContent.replace(/const currentTab = ref\('overview'\)/, scriptAdditions + '\nconst currentTab = ref(\'overview\')');

fs.writeFileSync(profilePath, profileContent);
console.log('Fixed ProfileDetail.vue');

// --- TeamDetail.vue ---
let teamPath = 'Frontend/src/views/HomeSite/Teams/TeamDetail.vue';
let teamContent = fs.readFileSync(teamPath, 'utf8');

let newTeamBio = `            <div class="bio-editor" v-if="isEditingBio">
              <RichTextEditor 
                v-model="tempBio" 
                @save="saveBio" 
                @cancel="cancelBio" 
                placeholder="Chia sẻ những gì nhóm bạn đang thực hiện" 
              />
            </div>
            <div class="bio-display tiptap-content" v-else @click="startEditingBio" style="cursor: pointer; min-height: 40px; padding: 8px; border-radius: 3px;" onmouseover="this.style.backgroundColor='#FAFBFC'" onmouseout="this.style.backgroundColor='transparent'">
              <div v-if="team.description && team.description !== '<p></p>'" v-html="team.description"></div>
              <p class="description-text" style="color: #5E6C84; margin: 0;" v-else>Chia sẻ những gì nhóm bạn đang thực hiện</p>
            </div>`;

teamContent = teamContent.replace(/<div class="bio-display" v-if="!isEditingBio"[\s\S]*?<\/div>\s*<\/div>/, newTeamBio);

if (!teamContent.includes('RichTextEditor.vue')) {
  teamContent = teamContent.replace(/import UserAvatar from '@\/components\/common\/UserAvatar\.vue'/, "import UserAvatar from '@/components/common/UserAvatar.vue'\nimport RichTextEditor from '@/components/common/RichTextEditor.vue'");
}

fs.writeFileSync(teamPath, teamContent);
console.log('Fixed TeamDetail.vue');

