const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

// 1. Refactor the Contingency Plan Tab List
const listStartIdx = content.indexOf('<div class="mb-6 contingency-plans-container" v-if="activityTab === \'contingency\'">');
const listEndIdx = content.indexOf('<div class="comment-box mb-6" v-if="activityTab === \'comments\'">');

if (listStartIdx !== -1 && listEndIdx !== -1) {
  const oldListBlock = content.substring(listStartIdx, listEndIdx);
  const newListBlock = `<div class="mb-6 contingency-plans-container" v-if="activityTab === 'contingency'">
              <div v-if="isLoadingContingency" class="text-center py-6 text-muted">
                 <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
                 <div class="mt-2 text-sm font-medium">Đang tải kế hoạch...</div>
              </div>
              <div v-else>
                  <div class="flex justify-between items-center mb-6">
                     <h3 class="cm-title" style="margin-bottom: 0;">Danh sách Kế hoạch Dự phòng</h3>
                     <button class="s-btn s-btn-primary px-4 py-2 text-[13px] font-bold rounded-lg shadow-sm flex items-center hover:-translate-y-0.5 transition-transform" @click="openCreateContingencyForm" v-if="contingencyPlans.length > 0">
                        <i class="fa-solid fa-plus mr-2"></i> Thêm Kế hoạch
                     </button>
                  </div>
                  
                  <div v-if="contingencyPlans.length > 0" class="flex flex-col gap-5">
                     <div class="bg-[var(--bg-primary)] rounded-xl border border-[var(--color-border)] shadow-sm hover:shadow-md transition-all duration-300 overflow-hidden" v-for="plan in contingencyPlans" :key="plan.id">
                        
                        <!-- Header Section -->
                        <div class="p-5 pb-4 border-b border-[var(--color-border)] bg-[var(--color-surface-hover)]/30">
                           <div class="flex justify-between items-start">
                              <div class="flex flex-col gap-2">
                                 <div class="flex items-center gap-3">
                                    <h4 class="text-base font-bold text-[var(--color-text-primary)] cursor-pointer hover:text-[var(--color-accent)] transition-colors" @click="openContingencyDetail(plan)">
                                       <i class="fa-solid fa-shield-halved text-[var(--color-text-muted)] mr-2"></i> 
                                       {{ plan.name }}
                                    </h4>
                                    <!-- Risk Badge -->
                                    <div class="cm-badge" style="cursor: default; padding: 2px 8px; font-size: 11px; text-transform: uppercase;" :style="{
                                           color: plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6',
                                           backgroundColor: \`color-mix(in srgb, \${plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6'} 15%, transparent)\`
                                        }">
                                       <i :class="plan.riskLevel === 'Critical' ? 'fa-solid fa-circle-exclamation' : plan.riskLevel === 'High' ? 'fa-solid fa-chevron-up' : plan.riskLevel === 'Medium' ? 'fa-solid fa-minus' : 'fa-solid fa-arrow-down'"></i>
                                       {{ plan.riskLevel || 'LOW' }}
                                    </div>
                                 </div>
                              </div>
                              <div class="flex gap-1" v-if="!plan.isActivated">
                                 <el-dropdown trigger="click" @command="(cmd) => handleContingencyAction(cmd, plan)">
                                    <button class="nav-icon-btn bg-transparent border-0 text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)]" type="button">
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
                           
                           <!-- Risk Description -->
                           <div class="mt-3 text-[13px] text-[var(--color-text-secondary)] leading-relaxed flex gap-2" v-if="plan.riskDescription">
                              <i class="fa-solid fa-triangle-exclamation mt-1" style="color: #ef4444;"></i>
                              <div class="whitespace-pre-wrap">{{ plan.riskDescription }}</div>
                           </div>
                        </div>

                        <!-- Task List Section -->
                        <div class="p-5 pt-4 bg-[var(--bg-primary)]">
                           <div class="flex justify-between items-center mb-4">
                              <div class="text-[12px] font-bold text-[var(--color-text-primary)] uppercase tracking-wider flex items-center">
                                 Task Dự Phòng <span class="ml-2 bg-[var(--color-surface-hover)] text-[var(--color-text-muted)] px-2 py-0.5 rounded-full text-[10px]">{{ plan.contingencyTasks?.length || 0 }}</span>
                              </div>
                              <button class="s-btn s-btn-secondary text-[12px] py-1.5 px-3 rounded-md flex items-center gap-1.5 font-semibold hover:bg-[var(--color-surface-hover)] transition-colors" @click="openCreateTaskForm(plan.id)" v-if="!plan.isActivated">
                                 <i class="fa-solid fa-plus"></i> Thêm Task
                              </button>
                           </div>

                           <div v-if="plan.contingencyTasks && plan.contingencyTasks.length > 0" class="grid grid-cols-1 gap-3">
                              <!-- Task Mini-Card -->
                              <div v-for="task in plan.contingencyTasks" :key="task.id" class="group flex flex-col md:flex-row md:items-center justify-between p-3.5 bg-[var(--bg-secondary)] border border-[var(--color-border)] rounded-lg hover:border-[var(--color-accent)] hover:shadow-sm transition-all duration-200">
                                 <div class="flex items-center gap-3 mb-3 md:mb-0">
                                    <div class="w-8 h-8 rounded-full bg-[var(--color-surface-hover)] flex items-center justify-center text-[var(--color-text-muted)] group-hover:text-[var(--color-accent)] transition-colors">
                                       <i class="fa-solid fa-list-check"></i>
                                    </div>
                                    <div class="flex flex-col">
                                       <span class="text-[14px] font-bold text-[var(--color-text-primary)] cursor-pointer hover:text-[var(--color-accent)] transition-colors" @click="openTaskDetail(task)">{{ task.title }}</span>
                                       <div class="flex items-center mt-1.5 gap-2">
                                          <div class="cm-badge" style="cursor: default; padding: 2px 6px; font-size: 10px; font-weight: 600;" :style="{ color: getStatusColor(task.statusName), backgroundColor: \`color-mix(in srgb, \${getStatusColor(task.statusName)} 15%, transparent)\` }">
                                             <i :class="getStatusIcon(task.statusName)"></i>
                                             {{ getStatusLabel(task.statusName) }}
                                          </div>
                                       </div>
                                    </div>
                                 </div>
                                 <div class="flex items-center">
                                    <button v-if="!task.isActivated && !plan.isActivated" class="s-btn s-btn-primary py-1.5 px-4 text-[12px] font-bold rounded-md shadow-sm w-full md:w-auto hover:-translate-y-0.5 transition-transform" @click.stop="confirmActivateTask(plan, task)">
                                       Kích hoạt
                                    </button>
                                    <div v-else-if="task.isActivated" class="cm-badge bg-green-50 text-green-600 border border-green-200 px-3 py-1.5 text-[11px] font-bold rounded-md flex items-center justify-center w-full md:w-auto">
                                       <i class="fa-solid fa-check mr-1.5"></i> Đã kích hoạt
                                    </div>
                                 </div>
                              </div>
                           </div>
                           <div v-else class="text-center py-6 border-2 border-dashed border-[var(--color-border)] rounded-lg bg-[var(--bg-secondary)]/50">
                              <p class="text-[13px] text-[var(--color-text-muted)] italic">Chưa có task dự phòng nào.</p>
                           </div>
                        </div>

                     </div>
                  </div>
                  
                  <div v-else class="text-center py-16 px-4 text-[var(--color-text-muted)] border-2 border-dashed border-[var(--color-border)] hover:border-[var(--color-accent)]/50 transition-colors rounded-2xl bg-[var(--bg-secondary)] flex flex-col items-center justify-center group cursor-pointer" @click="openCreateContingencyForm">
                     <div class="w-16 h-16 rounded-full bg-[var(--bg-primary)] shadow-sm flex items-center justify-center mb-5 group-hover:scale-110 transition-transform duration-300">
                        <i class="fa-solid fa-shield-halved text-3xl text-[var(--color-text-muted)] group-hover:text-[var(--color-accent)] transition-colors"></i>
                     </div>
                     <h4 class="text-[var(--color-text-primary)] text-lg font-bold mb-2">Chưa có kế hoạch dự phòng</h4>
                     <p class="text-[13px] opacity-80 mb-6 max-w-[320px] leading-relaxed">Một kế hoạch dự phòng giúp nhóm chuẩn bị sẵn các phương án fallback tự động khi công việc gặp rủi ro.</p>
                     <button class="s-btn s-btn-primary px-6 py-2.5 text-[13px] font-bold rounded-lg shadow-md hover:shadow-lg transition-all flex items-center hover:-translate-y-0.5">
                        <i class="fa-solid fa-plus mr-2"></i> Tạo kế hoạch đầu tiên
                     </button>
                  </div>
               </div>
            </div>
            
            `;
  content = content.replace(oldListBlock, newListBlock);
}

// 2. Refactor the Contingency Popup Modal
const popupStartIdx = content.indexOf('<transition name="fade">\\n  <div class="task-modal-overlay" v-if="showContingencyForm"');
const popupEndIdx = content.indexOf('<!-- Thêm Task Dự Phòng Dialog (Style giống Tạo Công Việc Mới) -->');

if (popupStartIdx === -1 || popupEndIdx === -1) {
  // Try regex or fallback
  const fallbackStart = content.indexOf('<div class="task-modal-overlay" v-if="showContingencyForm"');
  if (fallbackStart !== -1) {
     const realStart = content.lastIndexOf('<transition name="fade">', fallbackStart);
     if (realStart !== -1) {
        const realEnd = content.indexOf('<!-- Thêm Task', realStart);
        if (realEnd !== -1) {
           const oldPopup = content.substring(realStart, realEnd);
           const newPopup = `<transition name="fade">
  <div class="task-modal-overlay" v-if="showContingencyForm" @mousedown.self="showContingencyForm = false" style="z-index: 999999;">
    <div class="create-centered-modal transform transition-all scale-100 opacity-100" style="max-width: 550px; padding: 24px;">
      <div class="flex items-center justify-between mb-5">
         <h3 class="cm-title" style="margin-bottom: 0;">{{ editingContingencyPlanId ? 'Cập nhật Kế hoạch' : 'Kế hoạch dự phòng mới' }}</h3>
         <div class="cm-badge">
           <i class="fa-solid fa-shield-halved" style="color: #3b82f6"></i> DỰ PHÒNG
         </div>
      </div>

      <div class="cm-form-group flex flex-col gap-4 mb-4">
        <div>
           <label class="block text-[12px] font-bold text-[var(--color-text-secondary)] mb-1.5 ml-1 uppercase tracking-wide">Tên Kế hoạch</label>
           <input type="text" class="cm-inputbox transition-colors focus:border-[var(--color-accent)]" style="font-size: 14px; padding: 10px 14px;" placeholder="Nhập tên kế hoạch dự phòng..." v-model="contingencyPlanForm.name" />
        </div>
        
        <div>
           <label class="block text-[12px] font-bold text-[var(--color-text-secondary)] mb-1.5 ml-1 uppercase tracking-wide">Mô tả rủi ro</label>
           <textarea class="cm-textareabox !min-h-[80px] transition-colors focus:border-[var(--color-accent)]" style="font-size: 14px; padding: 10px 14px;" placeholder="Mô tả cụ thể rủi ro (VD: Khi server gặp sự cố...)" v-model="contingencyPlanForm.riskDescription"></textarea>
        </div>
      </div>

      <div class="cm-toolbar-row mb-5">
         <div class="w-full">
            <label class="block text-[12px] font-bold text-[var(--color-text-secondary)] mb-1.5 ml-1 uppercase tracking-wide">Mức độ nguy hiểm</label>
            <el-dropdown trigger="click" @command="(cmd) => contingencyPlanForm.riskLevel = cmd" class="w-full">
               <div class="t-btn w-full justify-between hover:bg-[var(--bg-secondary)] transition-colors" style="border: 1px solid var(--color-border); padding: 8px 14px; height: 42px; background: var(--bg-primary);">
                  <div class="flex items-center">
                     <i class="fa-solid fa-triangle-exclamation mr-2" :style="{color: contingencyPlanForm.riskLevel === 'Critical' ? '#ef4444' : contingencyPlanForm.riskLevel === 'High' ? '#f97316' : contingencyPlanForm.riskLevel === 'Medium' ? '#eab308' : '#3b82f6'}"></i> 
                     <span class="font-semibold">{{ contingencyPlanForm.riskLevel || 'Chọn mức độ' }}</span>
                  </div>
                  <i class="fa-solid fa-chevron-down text-[10px] text-gray-400"></i>
               </div>
               <template #dropdown>
                  <el-dropdown-menu class="theme-dropdown w-full" style="min-width: 200px;">
                     <el-dropdown-item command="Critical"><i class="fa-solid fa-circle-exclamation mr-2 text-red-500"></i> Critical (Nghiêm trọng)</el-dropdown-item>
                     <el-dropdown-item command="High"><i class="fa-solid fa-chevron-up mr-2 text-orange-500"></i> High (Cao)</el-dropdown-item>
                     <el-dropdown-item command="Medium"><i class="fa-solid fa-minus mr-2 text-yellow-500"></i> Medium (Trung bình)</el-dropdown-item>
                     <el-dropdown-item command="Low"><i class="fa-solid fa-arrow-down mr-2 text-blue-500"></i> Low (Thấp)</el-dropdown-item>
                  </el-dropdown-menu>
               </template>
            </el-dropdown>
         </div>
      </div>

      <div class="cm-form-group mb-6">
         <label class="block text-[12px] font-bold text-[var(--color-text-secondary)] mb-1.5 ml-1 uppercase tracking-wide">Ghi chú chi tiết</label>
         <div class="cm-editor-wrapper bg-[var(--bg-primary)] min-h-[120px] max-h-[250px] overflow-y-auto p-2 border border-[var(--color-border)] rounded-lg transition-colors focus-within:border-[var(--color-accent)] focus-within:ring-1 focus-within:ring-[var(--color-accent)]">
            <editor-content :editor="recoveryPlanEditor" />
         </div>
      </div>

      <div class="flex justify-end items-center gap-3 pt-2">
         <button class="px-5 py-2.5 rounded-lg text-[13px] font-bold text-[var(--color-text-secondary)] hover:bg-[var(--bg-secondary)] transition-colors" @click="showContingencyForm = false">Hủy bỏ</button>
         <button class="s-btn s-btn-primary px-6 py-2.5 text-[13px] font-bold rounded-lg shadow-md hover:shadow-lg transition-all" @click="saveContingencyPlan" :disabled="isSavingContingency">
            <i class="fa-solid fa-spinner fa-spin mr-2" v-if="isSavingContingency"></i>
            {{ editingContingencyPlanId ? 'Cập nhật' : 'Tạo kế hoạch' }}
         </button>
      </div>
    </div>
  </div>
</transition>

`;
           content = content.replace(oldPopup, newPopup);
        }
     }
  }
}

fs.writeFileSync(file, content, 'utf8');
console.log("Refactored successfully!");
