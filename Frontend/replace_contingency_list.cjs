const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const oldList = `<div class="flex justify-between items-center mb-4">
                     <h3 class="text-[14px] font-bold text-[var(--color-text-primary)]">Danh sách Kế hoạch Dự phòng</h3>
                     <button class="s-btn s-btn-primary px-4 py-2 text-[13px] font-bold rounded-lg shadow-sm hover:shadow-md transition-all flex items-center" @click="openCreateContingencyForm" v-if="contingencyPlans.length > 0">
                        <i class="fa-solid fa-plus mr-2"></i> Thêm Kế hoạch
                     </button>
                  </div>
                  
                  <div v-if="contingencyPlans.length > 0" class="flex flex-col gap-4">
                     <div class="contingency-card group hover:border-[var(--color-accent)] transition-all duration-300 relative overflow-hidden bg-[var(--bg-secondary)] border border-[var(--border-color)] rounded-xl p-5" v-for="plan in contingencyPlans" :key="plan.id">
                        
                        <div class="flex justify-between items-start mb-4">
                           <div>
                              <h4 class="text-[16px] font-bold text-[var(--color-text-primary)] mb-1.5 flex items-center cursor-pointer hover:text-[var(--color-accent)] transition-colors" @click="openContingencyDetail(plan)">
                                 <i class="fa-solid fa-shield-halved text-[var(--color-text-muted)] mr-2 text-sm"></i> 
                                 {{ plan.name }}
                              </h4>
                              <div class="flex flex-wrap items-center gap-2 mt-1">
                                 <span class="text-[10px] px-2.5 py-1 rounded-md font-bold tracking-wide uppercase border shadow-sm" :class="getRiskLevelClass(plan.riskLevel)">{{ plan.riskLevel }}</span>
                              </div>
                           </div>
                           <div class="flex gap-1" v-if="!plan.isActivated">
                              <el-dropdown trigger="click" @command="(cmd) => handleContingencyAction(cmd, plan)">
                                 <button class="text-[var(--color-text-muted)] hover:bg-[var(--bg-tertiary)] hover:text-[var(--color-text-primary)] w-8 h-8 rounded-lg flex items-center justify-center transition-all bg-transparent">
                                    <i class="fa-solid fa-ellipsis-vertical text-lg"></i>
                                 </button>
                                 <template #dropdown>
                                    <el-dropdown-menu class="theme-dropdown shadow-lg rounded-xl overflow-hidden border border-[var(--border-color)]">
                                       <el-dropdown-item command="edit" class="py-2.5 px-4 font-medium"><i class="fa-solid fa-pen mr-2 text-[var(--color-text-muted)]"></i> Chỉnh sửa</el-dropdown-item>
                                       <el-dropdown-item command="delete" class="py-2.5 px-4 font-medium text-red-600 hover:bg-red-50"><i class="fa-solid fa-trash mr-2"></i> Xóa kế hoạch</el-dropdown-item>
                                    </el-dropdown-menu>
                                 </template>
                              </el-dropdown>
                           </div>
                        </div>

                        <div class="bg-[var(--bg-primary)] rounded-lg p-3.5 mb-4 border border-[var(--border-color)]/60" v-if="plan.riskDescription">
                           <div class="text-[11px] font-bold text-[var(--color-text-muted)] uppercase tracking-wider mb-1.5 flex items-center"><i class="fa-solid fa-triangle-exclamation mr-1.5 text-[var(--color-danger)]"></i> Rủi ro (Cho rủi ro gì)</div>
                           <div class="text-[13px] text-[var(--color-text-primary)] font-medium leading-relaxed">
                              {{ plan.riskDescription }}
                           </div>
                        </div>

                        <!-- Danh sách Task dự phòng -->
                        <div class="mt-4">
                           <div class="flex justify-between items-center mb-2">
                              <div class="text-[12px] font-bold text-[var(--color-accent)] uppercase tracking-wider flex items-center"><i class="fa-solid fa-list-check mr-1.5"></i> Task Dự Phòng ({{ plan.contingencyTasks?.length || 0 }})</div>
                              <button class="text-[11px] font-bold text-[var(--color-accent)] hover:underline" @click="openCreateTaskForm(plan.id)" v-if="!plan.isActivated">+ Thêm Task</button>
                           </div>
                           <div v-if="plan.contingencyTasks && plan.contingencyTasks.length > 0" class="flex flex-col gap-2">
                              <div v-for="task in plan.contingencyTasks" :key="task.id" class="bg-[var(--bg-primary)] p-3 rounded-lg border border-[var(--border-color)] flex justify-between items-center">
                                 <div>
                                    <div class="font-semibold text-[13px] text-[var(--color-text-primary)]">{{ task.title }}</div>
                                    <div class="text-[11px] text-[var(--color-text-muted)] mt-1">Trạng thái: <span class="font-bold uppercase tracking-wider" :style="{color: getStatusColor(task.statusName)}">{{ task.statusName }}</span></div>
                                 </div>
                                 <div v-if="!task.isActivated && !plan.isActivated">
                                    <button class="text-[11px] px-3 py-1.5 rounded bg-[var(--color-accent)] text-white font-bold hover:shadow-md transition-all active:scale-95" @click="confirmActivateTask(plan, task)">
                                       Kích hoạt
                                    </button>
                                 </div>
                                 <div v-else-if="task.isActivated" class="text-[11px] font-bold text-green-600 flex items-center bg-green-50 px-2 py-1 rounded">
                                    <i class="fa-solid fa-check mr-1"></i> Đã kích hoạt
                                 </div>
                              </div>
                           </div>
                           <div v-else class="text-[12px] text-[var(--color-text-muted)] italic py-2">Chưa có task dự phòng nào.</div>
                        </div>

                     </div>
                  </div>`;

const newList = `<div class="flex justify-between items-center mb-4">
                     <h3 class="cm-title" style="margin-bottom: 0;">Danh sách Kế hoạch Dự phòng</h3>
                     <button class="cm-btn-primary" @click="openCreateContingencyForm" v-if="contingencyPlans.length > 0">
                        <i class="fa-solid fa-plus mr-2"></i> Thêm Kế hoạch
                     </button>
                  </div>
                  
                  <div v-if="contingencyPlans.length > 0" class="flex flex-col gap-4">
                     <div class="subtask-list p-4 rounded-lg border border-[var(--color-border)] bg-[var(--bg-secondary)]" style="display: flex; flex-direction: column; gap: 12px; margin: 0; box-shadow: var(--shadow-sm);" v-for="plan in contingencyPlans" :key="plan.id">
                        
                        <div class="flex justify-between items-start">
                           <div class="flex flex-col gap-1">
                              <h4 class="text-[15px] font-bold text-[var(--color-text-primary)] flex items-center cursor-pointer hover:text-[var(--color-accent)] transition-colors" @click="openContingencyDetail(plan)">
                                 <i class="fa-solid fa-shield-halved text-[var(--color-text-muted)] mr-2"></i> 
                                 {{ plan.name }}
                              </h4>
                              <div class="flex items-center gap-2 mt-1">
                                 <div class="subtask-chip" style="cursor: default;" :style="{
                                       color: plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6',
                                       borderColor: plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6',
                                       background: \`color-mix(in srgb, \${plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6'} 15%, transparent)\`
                                    }">
                                    <i :class="plan.riskLevel === 'Critical' ? 'fa-solid fa-circle-exclamation' : plan.riskLevel === 'High' ? 'fa-solid fa-chevron-up' : plan.riskLevel === 'Medium' ? 'fa-solid fa-minus' : 'fa-solid fa-arrow-down'"></i>
                                    <span style="margin-left: 4px; font-weight: bold; text-transform: uppercase;">{{ plan.riskLevel || 'LOW' }}</span>
                                 </div>
                              </div>
                           </div>
                           <div class="flex gap-1" v-if="!plan.isActivated">
                              <el-dropdown trigger="click" @command="(cmd) => handleContingencyAction(cmd, plan)">
                                 <button class="nav-icon-btn bg-transparent border-0" type="button">
                                    <i class="fa-solid fa-ellipsis-vertical text-lg"></i>
                                 </button>
                                 <template #dropdown>
                                    <el-dropdown-menu class="theme-dropdown">
                                       <el-dropdown-item command="edit"><i class="fa-solid fa-pen mr-2 text-[var(--color-text-muted)]"></i> Chỉnh sửa</el-dropdown-item>
                                       <el-dropdown-item command="delete" class="text-red-500"><i class="fa-solid fa-trash mr-2"></i> Xóa kế hoạch</el-dropdown-item>
                                    </el-dropdown-menu>
                                 </template>
                              </el-dropdown>
                           </div>
                        </div>

                        <div class="p-3 bg-[var(--bg-primary)] rounded border border-[var(--color-border)] mt-1" v-if="plan.riskDescription">
                           <div class="text-[12px] font-bold text-[var(--color-text-secondary)] uppercase mb-1 flex items-center">
                              <i class="fa-solid fa-triangle-exclamation mr-1.5" style="color: #ef4444;"></i> Rủi ro (Cho rủi ro gì)
                           </div>
                           <div class="text-[13px] text-[var(--color-text-primary)] whitespace-pre-wrap">{{ plan.riskDescription }}</div>
                        </div>

                        <!-- Danh sách Task dự phòng -->
                        <div class="mt-2">
                           <div class="flex justify-between items-center mb-2">
                              <div class="text-[12px] font-bold text-[var(--color-text-secondary)] uppercase flex items-center"><i class="fa-solid fa-list-check mr-1.5"></i> Task Dự Phòng ({{ plan.contingencyTasks?.length || 0 }})</div>
                              <button class="subtask-open" style="font-weight: bold; color: var(--color-accent);" @click="openCreateTaskForm(plan.id)" v-if="!plan.isActivated">+ Thêm Task</button>
                           </div>
                           <div v-if="plan.contingencyTasks && plan.contingencyTasks.length > 0" class="subtask-list" style="margin-top: 8px;">
                              <div v-for="task in plan.contingencyTasks" :key="task.id" class="subtask-item" style="border: 1px solid var(--color-border);">
                                 <div class="subtask-main">
                                    <button class="subtask-open" type="button" @click="openTaskDetail(task)">
                                       <span class="subtask-seq"><i class="fa-solid fa-shield-halved text-[var(--color-text-muted)] mr-1"></i></span>
                                       <span class="subtask-title">{{ task.title }}</span>
                                    </button>
                                    <div class="subtask-controls">
                                       <div class="subtask-chip" style="cursor: default;" :style="{ color: getStatusColor(task.statusName), borderColor: getStatusColor(task.statusName), background: \`color-mix(in srgb, \${getStatusColor(task.statusName)} 15%, transparent)\` }">
                                          <i :class="getStatusIcon(task.statusName)"></i>
                                          <span style="margin-left: 4px;">{{ getStatusLabel(task.statusName) }}</span>
                                       </div>

                                       <div v-if="!task.isActivated && !plan.isActivated">
                                          <button class="cm-btn-primary" style="padding: 2px 8px; font-size: 11px; height: 26px; border-radius: 4px;" @click.stop="confirmActivateTask(plan, task)">
                                             Kích hoạt
                                          </button>
                                       </div>
                                       <div v-else-if="task.isActivated" class="subtask-chip" style="color: #10b981; border-color: #10b981; background: color-mix(in srgb, #10b981 15%, transparent); cursor: default;">
                                          <i class="fa-solid fa-check"></i>
                                          <span style="margin-left: 4px;">Đã kích hoạt</span>
                                       </div>
                                    </div>
                                 </div>
                              </div>
                           </div>
                           <div v-else class="text-[12px] text-[var(--color-text-muted)] italic py-2">Chưa có task dự phòng nào.</div>
                        </div>

                     </div>
                  </div>`;

if(content.includes(oldList)) {
    content = content.replace(oldList, newList);
    fs.writeFileSync(file, content, 'utf8');
    console.log("Success");
} else {
    console.log("Not found exact match");
}
