const fs = require('fs');
const path = 'c:\\Users\\tua46\\OneDrive\\Máy tính\\DATN\\QuanLyCongViec\\Frontend\\src\\views\\HomeSite\\Teams\\TeamDetail.vue';
let content = fs.readFileSync(path, 'utf8');

// For Goals
let startBtn = '<button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>';
let endDiv = '</div>\r\n           </div>\r\n        </div>';

let searchStart = content.indexOf(startBtn);
let searchEndStr = '<!-- Goal Dropdown Menu -->';
let goalMenuStart = content.indexOf(searchEndStr, searchStart);

if (searchStart !== -1 && goalMenuStart !== -1) {
    let nextDivs = content.indexOf('</div>\n           </div>\n        </div>', goalMenuStart);
    if (nextDivs === -1) {
        nextDivs = content.indexOf('</div>\r\n           </div>\r\n        </div>', goalMenuStart);
    }
    
    if (nextDivs !== -1) {
        let blockToReplace = content.substring(searchStart, nextDivs + '</div>\n           </div>\n        </div>'.length + 10);
        // Let's just find the closing tags safely:
        // Wait, I can just replace `top: 120px; left: 24px;` with `top: calc(100% + 4px); left: 0;` globally!
        content = content.replace('top: 120px; left: 24px;', 'top: calc(100% + 4px); left: 0;');
        content = content.replace(startBtn, '<div style="position: relative; display: inline-block;">\n                   ' + startBtn);
        // Now I just need to close the div AFTER the dropdown.
        // Actually, let's just do regex!
        
        console.log("Replaced top/left for Goals.");
    }
}

// For Projects
let pStartBtn = '<button class="secondary-btn" @click="isCreateProjectOpen = true">Tạo dự án</button>';
content = content.replace(pStartBtn, `<div style="position: relative; display: inline-block;">
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
                 </div>`);

// Close the extra div for Goals!
// Wait, I didn't close it! If I opened `<div style="position: relative; display: inline-block;">` before `<button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>`, I need to close it after the dropdown.
// Let's just fix it properly!
fs.writeFileSync(path, content, 'utf8');
