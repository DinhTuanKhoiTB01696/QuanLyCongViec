const fs = require('fs');

const path = 'c:\\Users\\tua46\\OneDrive\\Máy tính\\DATN\\QuanLyCongViec\\Frontend\\src\\views\\HomeSite\\Teams\\TeamDetail.vue';
let content = fs.readFileSync(path, 'utf8');

const oldStr = `                 <button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>
              </div>

              <!-- Goal Dropdown Menu -->
              <div class="dropdown-menu search-dropdown" v-if="isGoalDropdownOpen" @click.stop style="position: absolute; top: 120px; left: 24px; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">
                 <input type="text" v-model="goalSearch" placeholder="Tìm kiếm mục tiêu hoặc dán liên kết" class="search-input" style="width: 100%; margin-bottom: 12px; padding-left: 12px !important;" />
                 <h5 style="font-size: 11px; color: #6B778C; text-transform: uppercase; padding: 0 8px 8px;">Mục tiêu gần đây</h5>
                 <div class="goal-list-options" style="max-height: 200px; overflow-y: auto;">
                   <div class="team-option" v-for="g in mockRecentGoals" :key="g.id" @click="linkGoal(g)" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; border-radius: 3px;">
                     <i class="fa-solid fa-bullseye" style="color: #6B778C; font-size: 14px;"></i>
                     <div style="display: flex; flex-direction: column;">
                       <span style="font-size: 13px; color: #172B4D;">{{ g.title }}</span>
                       <span style="font-size: 11px; color: #6B778C;">{{ g.owner }}</span>
                     </div>
                   </div>
                 </div>
                 <div style="border-top: 1px solid #DFE1E6; margin-top: 8px; padding-top: 8px;">
                    <div class="team-option" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; color: #172B4D;" @click="isCreateGoalOpen = true">
                       <i class="fa-solid fa-plus"></i> <span style="font-size: 13px;">Tạo mục tiêu</span>
                    </div>
                 </div>
              </div>
           </div>
        </div>`;

const newStr = `                 <div style="position: relative; display: inline-block;">
                   <button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>
                   
                   <!-- Goal Dropdown Menu -->
                   <div class="dropdown-menu search-dropdown" v-if="isGoalDropdownOpen" @click.stop style="position: absolute; top: calc(100% + 4px); left: 0; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">
                      <input type="text" v-model="goalSearch" placeholder="Tìm kiếm mục tiêu hoặc dán liên kết" class="search-input" style="width: 100%; margin-bottom: 12px; padding-left: 12px !important;" />
                      <h5 style="font-size: 11px; color: #6B778C; text-transform: uppercase; padding: 0 8px 8px;">Mục tiêu gần đây</h5>
                      <div class="goal-list-options" style="max-height: 200px; overflow-y: auto;">
                        <div class="team-option" v-for="g in siteGoals" :key="g.id" @click="linkGoal(g)" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; border-radius: 3px;">
                          <i class="fa-solid fa-bullseye" style="color: #6B778C; font-size: 14px;"></i>
                          <div style="display: flex; flex-direction: column;">
                            <span style="font-size: 13px; color: #172B4D;">{{ g.title }}</span>
                            <span style="font-size: 11px; color: #6B778C;">{{ g.owner }}</span>
                          </div>
                        </div>
                        <div v-if="siteGoals.length === 0" style="padding: 8px; font-size: 12px; color: #6B778C;">Không tìm thấy mục tiêu nào.</div>
                      </div>
                      <div style="border-top: 1px solid #DFE1E6; margin-top: 8px; padding-top: 8px;">
                         <div class="team-option" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; color: #172B4D;" @click="isCreateGoalOpen = true">
                            <i class="fa-solid fa-plus"></i> <span style="font-size: 13px;">Tạo mục tiêu</span>
                         </div>
                      </div>
                   </div>
                 </div>
              </div>
           </div>
        </div>`;

if (content.includes(oldStr)) {
    content = content.replace(oldStr, newStr);
    console.log("Goals tab dropdown replaced successfully.");
} else {
    console.log("Could not find oldStr for Goals. Trying alternative...");
    // Let's print out what we found
    console.log(content.indexOf('<!-- Goal Dropdown Menu -->'));
}

const oldProjStr = `                 <button class="secondary-btn" @click="isCreateProjectOpen = true">Tạo dự án</button>
              </div>
           </div>
        </div>`;

const newProjStr = `                 <div style="position: relative; display: inline-block;">
                    <button class="secondary-btn" @click.stop="isProjectDropdownOpen = !isProjectDropdownOpen">Thêm dự án</button>
                    <!-- Project Dropdown Menu -->
                    <div class="dropdown-menu search-dropdown" v-if="isProjectDropdownOpen" @click.stop style="position: absolute; top: calc(100% + 4px); left: 0; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">
                       <input type="text" v-model="projectSearch" placeholder="Tìm kiếm dự án hoặc dán liên kết" class="search-input" style="width: 100%; margin-bottom: 12px; padding-left: 12px !important;" />
                       <h5 style="font-size: 11px; color: #6B778C; text-transform: uppercase; padding: 0 8px 8px;">Dự án gần đây</h5>
                       <div class="goal-list-options" style="max-height: 200px; overflow-y: auto;">
                         <div class="team-option" v-for="p in siteProjects" :key="p.id" @click="linkProject(p)" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; border-radius: 3px;">
                           <div style="width: 16px; height: 16px; background-color: #0052cc; border-radius: 2px; flex-shrink: 0;"></div>
                           <div style="display: flex; flex-direction: column;">
                             <span style="font-size: 13px; color: #172B4D;">{{ p.name || p.title }}</span>
                           </div>
                         </div>
                         <div v-if="siteProjects.length === 0" style="padding: 8px; font-size: 12px; color: #6B778C;">Không tìm thấy dự án nào.</div>
                       </div>
                       <div style="border-top: 1px solid #DFE1E6; margin-top: 8px; padding-top: 8px;">
                          <div class="team-option" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; color: #172B4D;" @click="isCreateProjectOpen = true">
                             <i class="fa-solid fa-plus"></i> <span style="font-size: 13px;">Tạo dự án mới</span>
                          </div>
                       </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>`;

if (content.includes(oldProjStr)) {
    content = content.replace(oldProjStr, newProjStr);
    console.log("Projects tab dropdown replaced successfully.");
} else {
    console.log("Could not find oldProjStr for Projects.");
}

fs.writeFileSync(path, content, 'utf8');
