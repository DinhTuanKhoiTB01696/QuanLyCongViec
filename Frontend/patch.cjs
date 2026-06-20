const fs = require('fs');
let content = fs.readFileSync('src/views/HomeSite/Goals/GoalDetail.vue', 'utf-8');

// 1. Replace tab names
content = content.replace(
  '<button class="tab-btn" :class="{ active: currentTab === \'projects\' }" @click="currentTab = \'projects\'">Dự án</button>',
  '<button class="tab-btn" :class="{ active: currentTab === \'space-projects\' }" @click="currentTab = \'space-projects\'\">SprintA</button>\n        <button class="tab-btn" :class="{ active: currentTab === \'site-projects\' }" @click="currentTab = \'site-projects\'\">Project của Site</button>'
);

// 2. Replace the Projects tab template
const projectsTabStart = '<!-- DỰ ÁN TAB -->';
const learningsTabStart = '<!-- BÀI HỌC RÚT RA TAB -->';
const projectsTabContent = content.substring(content.indexOf(projectsTabStart), content.indexOf(learningsTabStart));

const newTabsContent = `<!-- SPRINTA TAB -->
        <template v-if="currentTab === 'space-projects'">
          <div style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 32px; background: white; margin-top: 16px;">
            <div style="display: flex; gap: 24px; max-width: 600px; margin: 0 auto; position: relative;">
               <div style="width: 64px; height: 64px; background: #EBECF0; color: #172B4D; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 32px; position: relative; flex-shrink: 0;">
                  <Rocket class="w-4 h-4"></Rocket>
                  <div style="position: absolute; bottom: -8px; right: -8px; width: 24px; height: 24px; background: #0052CC; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 14px; border: 2px solid white;">
                     <Plus class="w-4 h-4"></Plus>
                  </div>
               </div>
               <div>
                 <h3 style="margin: 0 0 8px 0; font-size: 16px; color: #172B4D;">Thêm dự án SprintA để sắp xếp công việc</h3>
                 <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Kết nối với các project nằm trong workspace hiện tại của bạn.</p>
                 
                 <div v-if="goalStore.spaceProjects.length > 0" style="margin-bottom: 16px;">
                   <div v-for="proj in goalStore.spaceProjects" :key="proj.id" style="display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; border: 1px solid #DFE1E6; border-radius: 3px; margin-bottom: 8px;">
                     <div style="display: flex; align-items: center; gap: 8px;">
                       <div style="width: 24px; height: 24px; background: #FFAB00; border-radius: 3px; display: flex; align-items: center; justify-content: center;"><Rocket class="w-4 h-4" style="color: white;"></Rocket></div>
                       <span style="font-size: 14px; font-weight: 500; color: #172B4D;">{{ proj.name }}</span>
                     </div>
                     <button class="remove-btn icon-only" @click="unlinkGoalProject(proj.id, 'spaceProject')"><X class="w-4 h-4"></X></button>
                   </div>
                 </div>

                 <div style="position: relative; display: inline-block;">
                   <button class="secondary-btn" @click="isSpaceProjectSearchOpen = !isSpaceProjectSearchOpen" style="background: white; border: 1px solid #DFE1E6; font-weight: 600;">Thêm dự án SprintA</button>
                   
                   <div v-if="isSpaceProjectSearchOpen" class="dropdown-menu" style="position: absolute; top: 100%; left: 0; margin-top: 8px; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); width: 320px; z-index: 100; padding: 12px 0;">
                      <div style="padding: 0 12px 12px;">
                        <input type="text" placeholder="ID project tạm (để test)" v-model="tempSpaceProjectId" style="width: 100%; padding: 8px 12px; border: 2px solid #DFE1E6; border-radius: 3px; font-size: 14px; outline: none; box-sizing: border-box;" />
                      </div>
                      <div style="padding: 0 12px;">
                        <button class="primary-btn" @click="addGoalProject(tempSpaceProjectId, 'spaceProject')" style="width: 100%;">Thêm</button>
                      </div>
                   </div>
                 </div>
               </div>
            </div>
          </div>
        </template>

        <!-- SITE PROJECTS TAB -->
        <template v-if="currentTab === 'site-projects'">
          <div style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 32px; background: white; margin-top: 16px;">
            <div style="display: flex; gap: 24px; max-width: 600px; margin: 0 auto; position: relative;">
               <div style="width: 64px; height: 64px; background: #E3FCEF; color: #006644; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 32px; position: relative; flex-shrink: 0;">
                  <Target class="w-4 h-4"></Target>
                  <div style="position: absolute; bottom: -8px; right: -8px; width: 24px; height: 24px; background: #36B37E; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 14px; border: 2px solid white;">
                     <Plus class="w-4 h-4"></Plus>
                  </div>
               </div>
               <div>
                 <h3 style="margin: 0 0 8px 0; font-size: 16px; color: #172B4D;">Thêm dự án của Site vào mục tiêu này</h3>
                 <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Kết nối với các project chung của toàn bộ Site.</p>
                 
                 <div v-if="goalStore.siteProjects.length > 0" style="margin-bottom: 16px;">
                   <div v-for="proj in goalStore.siteProjects" :key="proj.id" style="display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; border: 1px solid #DFE1E6; border-radius: 3px; margin-bottom: 8px;">
                     <div style="display: flex; align-items: center; gap: 8px;">
                       <div style="width: 24px; height: 24px; background: #36B37E; border-radius: 3px; display: flex; align-items: center; justify-content: center;"><Target class="w-4 h-4" style="color: white;"></Target></div>
                       <span style="font-size: 14px; font-weight: 500; color: #172B4D;">{{ proj.name }}</span>
                     </div>
                     <button class="remove-btn icon-only" @click="unlinkGoalProject(proj.id, 'siteProject')"><X class="w-4 h-4"></X></button>
                   </div>
                 </div>

                 <div style="position: relative; display: inline-block;">
                   <button class="secondary-btn" @click="isSiteProjectSearchOpen = !isSiteProjectSearchOpen" style="background: white; border: 1px solid #DFE1E6; font-weight: 600;">Thêm dự án Site</button>
                   
                   <div v-if="isSiteProjectSearchOpen" class="dropdown-menu" style="position: absolute; top: 100%; left: 0; margin-top: 8px; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); width: 320px; z-index: 100; padding: 12px 0;">
                      <div style="padding: 0 12px 12px;">
                        <input type="text" placeholder="ID project tạm (để test)" v-model="tempSiteProjectId" style="width: 100%; padding: 8px 12px; border: 2px solid #DFE1E6; border-radius: 3px; font-size: 14px; outline: none; box-sizing: border-box;" />
                      </div>
                      <div style="padding: 0 12px;">
                        <button class="primary-btn" @click="addGoalProject(tempSiteProjectId, 'siteProject')" style="width: 100%;">Thêm</button>
                      </div>
                   </div>
                 </div>
               </div>
            </div>
          </div>
        </template>
        
        `;

content = content.replace(projectsTabContent, newTabsContent);

// 3. Replace setup vars
const oldSetup = `const isJiraInputOpen = ref(false)
const isProjectSearchOpen = ref(false)`;

const newSetup = `const isJiraInputOpen = ref(false)
const isSpaceProjectSearchOpen = ref(false)
const isSiteProjectSearchOpen = ref(false)
const tempSpaceProjectId = ref('')
const tempSiteProjectId = ref('')

const addGoalProject = async (projectId, linkType) => {
  if (!projectId || !goal.value) return
  try {
    await goalStore.addProjectLink(goal.value.id, projectId, linkType)
    tempSpaceProjectId.value = ''
    tempSiteProjectId.value = ''
    isSpaceProjectSearchOpen.value = false
    isSiteProjectSearchOpen.value = false
  } catch (err) {
    alert(err.response?.data?.message || err.message)
  }
}

const unlinkGoalProject = async (linkId, linkType) => {
  if (!goal.value) return
  try {
    await goalStore.deleteProjectLink(goal.value.id, linkId)
  } catch (err) {
    alert(err.response?.data?.message || err.message)
  }
}`;

content = content.replace(oldSetup, newSetup);

fs.writeFileSync('src/views/HomeSite/Goals/GoalDetail.vue', content);
