<template>
  <div class="team-kudos-container">
    <div class="kudos-empty-state">
      <div class="kudos-illustration">
        <div class="mock-star-medal">
          <i class="fa-solid fa-star"></i>
          <div class="ribbon ribbon-left"></div>
          <div class="ribbon ribbon-right"></div>
        </div>
      </div>
      <h2>Không có lời khen ngợi nào gần đây</h2>
      <p>Gửi lời khen ngợi để cảm ơn một thành viên trong nhóm, ăn mừng một chiến thắng nhỏ hoặc ghi nhận một công việc được hoàn thành xuất sắc.</p>
      <button class="primary-btn" @click="isGiveKudosOpen = true">Gửi lời khen ngợi</button>
    </div>

    <!-- Give Kudos Full Screen Overlay -->
    <div class="give-kudos-overlay" v-if="isGiveKudosOpen" style="position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: #FFF4F8; z-index: 1000; overflow-y: auto; display: flex; flex-direction: column; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;">
       
       <!-- Header -->
       <div style="padding: 16px 24px; display: flex; justify-content: space-between; align-items: center;">
          <button class="icon-btn" @click="isGiveKudosOpen = false" style="background: transparent; border: none; font-size: 16px; cursor: pointer; color: #42526E;"><i class="fa-solid fa-arrow-left"></i></button>
          
          <div></div> <!-- Empty center to push button right -->

          <button class="primary-btn" :disabled="!kudosText" style="height: 32px;" @click="submitKudos">Khen ngợi</button>
       </div>

       <!-- Content -->
       <div style="flex: 1; display: flex; justify-content: center; padding-top: 40px;" @click="isKudosLinkDropdownOpen = false; isKudosTargetDropdownOpen = false; isKudosEmojiDropdownOpen = false">
          <div style="width: 100%; max-width: 600px; display: flex; flex-direction: column; gap: 24px; position: relative;">
             <div style="position: relative;">
                 <div style="display: flex; align-items: center; gap: 8px; font-weight: 500; font-size: 14px; color: #0052CC; cursor: pointer; padding: 8px 12px; border: 1px solid #4C9AFF; border-radius: 4px; display: inline-flex;" @click.stop="isKudosTargetDropdownOpen = !isKudosTargetDropdownOpen">
                    <div class="member-avatar-micro" :style="{ backgroundColor: kudosTargetType === 'team' ? '#E2B203' : '#0052CC', color: 'white', width: '24px', height: '24px', borderRadius: kudosTargetType === 'team' ? '4px' : '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '11px' }">{{ kudosTargetAvatar }}</div>
                    Khen ngợi {{ kudosTargetName }}
                 </div>
                 
                 <!-- Target Dropdown -->
                 <div v-if="isKudosTargetDropdownOpen" @click.stop class="dropdown-menu" style="position: absolute; top: 40px; left: 0; z-index: 10; width: 340px; background: white; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border: 1px solid #DFE1E6; padding: 8px 0; display: flex; flex-direction: column; max-height: 300px; overflow-y: auto;">
                    <div style="padding: 4px 12px; font-size: 11px; font-weight: 700; color: #5E6C84; text-transform: uppercase;">Mọi người</div>
                    <div v-for="user in mockPeopleList" :key="user.id" @click="selectKudosTarget('user', user)" style="display: flex; align-items: center; gap: 8px; padding: 8px 16px; cursor: pointer; transition: background 0.1s;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                       <div class="member-avatar-micro" style="background-color: #0052CC; color: white; width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px;">{{ user.initials }}</div>
                       <span style="font-size: 14px; color: #172B4D;">{{ user.name }}</span>
                    </div>
                    <div style="padding: 4px 12px; font-size: 11px; font-weight: 700; color: #5E6C84; text-transform: uppercase; margin-top: 8px; border-top: 1px solid #DFE1E6; padding-top: 8px;">Đội ngũ</div>
                    <div v-for="t in mockTeamList" :key="t.id" @click="selectKudosTarget('team', t)" style="display: flex; align-items: center; gap: 8px; padding: 8px 16px; cursor: pointer; transition: background 0.1s; background: #E6FCFF;" onmouseover="this.style.background='#B3F5FF'" onmouseout="this.style.background='#E6FCFF'">
                       <div class="member-avatar-micro" style="background-color: #36B37E; color: white; width: 24px; height: 24px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px;">{{ t.initials }}</div>
                       <div style="display: flex; flex-direction: column;">
                         <span style="font-size: 14px; color: #0052CC;">{{ t.name }} <i class="fa-solid fa-circle-check" style="font-size: 10px;"></i></span>
                         <span style="font-size: 11px; color: #6B778C;">Đội ngũ chính thức • {{ t.memberCount }} thành viên, kể cả bạn</span>
                       </div>
                    </div>
                 </div>
             </div>
             
             <!-- Text input that renders HTML or handles link replacement -->
             <div style="position: relative;">
                 <textarea 
                   v-if="!isKudosRichText"
                   v-model="kudosText"
                   style="width: 100%; min-height: 60px; font-size: 20px; color: #172B4D; outline: none; border: none; background: transparent; line-height: 1.5; padding: 8px 0; resize: none; overflow: hidden; font-family: inherit; font-weight: 400;"
                   :placeholder="`Hãy cho ${kudosTargetName} biết lý do bạn gửi lời khen ngợi này`"
                   rows="1"
                   oninput="this.style.height = ''; this.style.height = this.scrollHeight + 'px'"
                 ></textarea>
                 <div v-else style="min-height: 60px; font-size: 20px; color: #172B4D; line-height: 1.5; padding: 8px 0; font-weight: 400;" @click="isKudosRichText = false">
                    {{ kudosTextBefore }}<a href="#" style="color: #0052CC; text-decoration: none;">{{ kudosLinkText }}</a>{{ kudosTextAfter }}
                 </div>
             </div>

             <!-- Icons toolbar -->
             <div style="display: flex; gap: 16px; color: #6B778C; font-size: 18px; align-items: center;">
               <div style="position: relative;">
                 <i class="fa-regular fa-face-smile" style="cursor: pointer;" @click.stop="isKudosEmojiDropdownOpen = !isKudosEmojiDropdownOpen"></i>
                 
                 <!-- Emoji Dropdown -->
                 <div v-if="isKudosEmojiDropdownOpen" @click.stop class="dropdown-menu" style="position: absolute; top: 28px; left: 0; z-index: 10; background: white; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border: 1px solid #DFE1E6; padding: 8px; display: grid; grid-template-columns: repeat(6, 1fr); gap: 4px;">
                    <div v-for="emoji in ['😀','🎉','👍','🚀','❤️','🔥','👏','🙌','💯','💪','✨','🌟']" :key="emoji" @click="insertEmoji(emoji)" style="cursor: pointer; font-size: 20px; text-align: center; padding: 4px; border-radius: 4px; transition: background 0.1s;" onmouseover="this.style.background='#F4F5F7'" onmouseout="this.style.background='transparent'">
                       {{ emoji }}
                    </div>
                 </div>
               </div>
               <div style="position: relative;">
                 <i class="fa-solid fa-link" style="cursor: pointer;" @click.stop="isKudosLinkDropdownOpen = !isKudosLinkDropdownOpen"></i>
                 
                 <!-- Link Dropdown -->
                 <div v-if="isKudosLinkDropdownOpen" @click.stop class="dropdown-menu" style="position: absolute; top: 24px; left: 0; z-index: 10; width: 340px; background: white; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border: 1px solid #DFE1E6; padding: 12px; display: flex; flex-direction: column; gap: 12px;">
                    <div>
                      <label style="font-size: 11px; font-weight: 600; color: #6B778C;">Tìm kiếm hoặc dán liên kết *</label>
                      <input type="text" placeholder="Tìm các liên kết gần đây hoặc dán một liên kết" style="width: 100%; margin-top: 4px; padding: 8px; border: 2px solid #4C9AFF; border-radius: 3px; outline: none; box-sizing: border-box;" v-model="kudosLinkSearch" />
                    </div>
                    <div>
                      <label style="font-size: 11px; font-weight: 600; color: #6B778C;">Văn bản hiển thị (không bắt buộc)</label>
                      <input type="text" placeholder="Văn bản cần hiển thị" style="width: 100%; margin-top: 4px; padding: 8px; border: 1px solid #DFE1E6; border-radius: 3px; outline: none; box-sizing: border-box;" v-model="kudosLinkDisplay" />
                      <div style="font-size: 11px; color: #6B778C; margin-top: 4px;">Cung cấp tiêu đề hoặc mô tả cho liên kết này</div>
                    </div>
                    
                    <div style="display: flex; gap: 16px; border-bottom: 1px solid #DFE1E6; padding-bottom: 8px;">
                      <span style="font-size: 13px; font-weight: 600; color: #0052CC; border-bottom: 2px solid #0052CC; padding-bottom: 8px; cursor: pointer; margin-bottom: -9px;">Home</span>
                      <span style="font-size: 13px; font-weight: 500; color: #6B778C; cursor: pointer;">Jira</span>
                      <span style="font-size: 13px; font-weight: 500; color: #6B778C; cursor: pointer;">Confluence</span>
                    </div>

                    <div>
                      <h5 style="font-size: 11px; color: #6B778C; text-transform: uppercase; margin-bottom: 8px; margin-top: 0;">Đã xem gần đây</h5>
                      <div style="max-height: 150px; overflow-y: auto; display: flex; flex-direction: column; gap: 4px;">
                        <div v-for="item in mockRecentLinks" :key="item.id" @click="selectKudosLink(item)" style="display: flex; align-items: flex-start; gap: 8px; padding: 4px; cursor: pointer; border-radius: 3px; transition: background 0.1s;" onmouseover="this.style.background='#F4F5F7'" onmouseout="this.style.background='transparent'">
                          <i :class="item.icon" :style="{ color: item.iconColor, marginTop: '4px' }"></i>
                          <div style="display: flex; flex-direction: column;">
                            <span style="font-size: 13px; color: #172B4D;">{{ item.title }}</span>
                            <span style="font-size: 11px; color: #6B778C;">{{ item.subtitle }}</span>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div style="display: flex; justify-content: flex-end; gap: 8px; margin-top: 8px;">
                      <button class="secondary-btn" @click="isKudosLinkDropdownOpen = false" style="height: 32px; background: transparent; border: none; color: #42526E; cursor: pointer; font-weight: 500; padding: 0 12px;">Hủy</button>
                      <button class="primary-btn" @click="insertKudosLink" style="height: 32px;">Chèn</button>
                    </div>
                 </div>
               </div>
             </div>

             <!-- Personalize Graphic Card -->
             <div style="width: 100%; height: 280px; background: #0052CC; border-radius: 8px; position: relative; overflow: hidden; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 12px rgba(0,0,0,0.1);">
                <button class="secondary-btn" style="position: absolute; top: 12px; right: 12px; font-size: 12px; padding: 4px 8px; height: auto; background: rgba(255,255,255,0.2); color: white; border: none; cursor: pointer; border-radius: 3px;">Cá nhân hóa</button>
                <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; position: relative;">
                   <i class="fa-solid fa-fish-fins" style="font-size: 40px; color: #FF8F73; position: absolute; right: -40px; top: -20px; transform: rotate(-15deg);"></i>
                   <i class="fa-solid fa-box-open" style="font-size: 100px; color: #FF5630; filter: drop-shadow(0 10px 10px rgba(0,0,0,0.2));"></i>
                   <div style="display: flex; gap: 8px; margin-top: -10px; z-index: -1;">
                     <i class="fa-solid fa-coins" style="font-size: 30px; color: #FFAB00;"></i>
                     <i class="fa-solid fa-coins" style="font-size: 40px; color: #FFAB00; margin-top: -10px;"></i>
                     <i class="fa-solid fa-coins" style="font-size: 30px; color: #FFAB00;"></i>
                   </div>
                </div>
             </div>
          </div>
       </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const isGiveKudosOpen = ref(false)
const kudosText = ref('')
const isKudosLinkDropdownOpen = ref(false)
const isKudosTargetDropdownOpen = ref(false)
const isKudosEmojiDropdownOpen = ref(false)
const kudosLinkSearch = ref('')
const kudosLinkDisplay = ref('')
const isKudosRichText = ref(false)
const kudosTextBefore = ref('')
const kudosLinkText = ref('')
const kudosTextAfter = ref('')

const kudosTargetType = ref('team')
const kudosTargetName = ref('Đội ngũ của bạn')
const kudosTargetAvatar = ref('Đ')

const mockPeopleList = [
  { id: 'u1', name: 'Tuấn Khôi Đinh', initials: 'TK' },
  { id: 'u2', name: 'ngkiet2805', initials: 'N' },
  { id: 'u3', name: 'Thịnh Phát Bùi', initials: 'TP' },
  { id: 'u4', name: 'Anh Quan Ng Hoang', initials: 'AQ' },
  { id: 'u5', name: 'Quân Đạt Võ', initials: 'QĐ' }
]

const mockTeamList = [
  { id: 't1', name: 'Nhóm phát triển', initials: 'NP', memberCount: 5 },
  { id: 't2', name: 'Dự án tốt nghiệp', initials: 'DA', memberCount: 6 }
]

const selectKudosTarget = (type, item) => {
  kudosTargetType.value = type
  kudosTargetName.value = item.name
  kudosTargetAvatar.value = item.initials
  isKudosTargetDropdownOpen.value = false
}

const insertEmoji = (emoji) => {
  kudosText.value = (kudosText.value || '') + emoji
  isKudosEmojiDropdownOpen.value = false
}

const mockRecentLinks = ref([
  { id: 1, title: 'Đặc tả lại dự án', subtitle: 'Dự Án Tốt Nghiệp • Đã xem 4 ngày trước', icon: 'fa-regular fa-square-check', iconColor: '#4C9AFF' },
  { id: 2, title: 'Làm lại tài liệu dự án', subtitle: 'Dự Án Tốt Nghiệp • Đã xem 4 ngày trước', icon: 'fa-regular fa-square-check', iconColor: '#4C9AFF' },
  { id: 3, title: 'Làm trang quản lý riêng cho space', subtitle: 'Dự Án Tốt Nghiệp • Đã xem 6 ngày trước', icon: 'fa-solid fa-code-branch', iconColor: '#4C9AFF' }
])

const selectKudosLink = (item) => {
  kudosLinkSearch.value = item.title
  kudosLinkDisplay.value = item.title
}

const insertKudosLink = () => {
  if (kudosLinkDisplay.value) {
    kudosTextBefore.value = (kudosText.value || '') + ' '
    kudosLinkText.value = kudosLinkDisplay.value
    kudosTextAfter.value = ' '
    isKudosRichText.value = true
    kudosText.value = kudosTextBefore.value + `<a href="/home/projects" style="color: #0052CC; text-decoration: none;">${kudosLinkText.value}</a>` + kudosTextAfter.value
    isKudosLinkDropdownOpen.value = false
  }
}

const submitKudos = () => {
  isGiveKudosOpen.value = false
  kudosText.value = ''
  isKudosRichText.value = false
}
</script>

<style scoped>
.team-kudos-container {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  display: flex;
  justify-content: center;
  padding-top: 64px;
}

.kudos-empty-state {
  max-width: 480px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.kudos-illustration {
  margin-bottom: 24px;
}

.mock-star-medal {
  position: relative;
  width: 80px;
  height: 80px;
  background-color: #FFC400;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2;
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
  margin: 0 auto;
}

.mock-star-medal i {
  font-size: 40px;
  color: #172B4D;
}

.ribbon {
  position: absolute;
  width: 20px;
  height: 40px;
  background-color: #0052CC;
  z-index: -1;
  top: 60px;
}

.ribbon-left {
  left: 10px;
  transform: rotate(30deg);
  clip-path: polygon(0 0, 100% 0, 100% 100%, 50% 80%, 0 100%);
}

.ribbon-right {
  right: 10px;
  transform: rotate(-30deg);
  clip-path: polygon(0 0, 100% 0, 100% 100%, 50% 80%, 0 100%);
}

.kudos-empty-state h2 {
  font-size: 20px;
  font-weight: 500;
  color: #172B4D;
  margin: 0 0 16px 0;
}

.kudos-empty-state p {
  font-size: 14px;
  color: #5E6C84;
  line-height: 1.5;
  margin: 0 0 24px 0;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
}

.primary-btn:hover {
  background-color: #0047B3;
}
</style>
