const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

// 1. Fix Z-index bằng Global CSS
const cssFix = `
<style>
/* FORCE EL-DIALOG & EL-DRAWER Z-INDEX TO BE ON TOP OF TASK MODAL */
body .el-overlay {
    z-index: 99999 !important;
}
body .el-message-box__wrapper {
    z-index: 999999 !important;
}
.contingency-plans-container .contingency-card {
    padding: 20px !important;
    border-radius: 12px !important;
    border: 1px solid var(--border-color);
    margin-bottom: 16px;
    background: var(--bg-primary);
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}
.contingency-plans-container .contingency-card-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 16px;
}
.contingency-plans-container .contingency-trigger-box {
    background: var(--bg-secondary);
    padding: 12px;
    border-radius: 8px;
    margin-bottom: 16px;
    border: 1px solid var(--border-color);
}
.contingency-plans-container .contingency-task-box {
    background: color-mix(in srgb, var(--color-accent) 5%, transparent);
    padding: 12px;
    border-radius: 8px;
    margin-bottom: 16px;
    border: 1px solid color-mix(in srgb, var(--color-accent) 20%, transparent);
}
.contingency-plans-container .contingency-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding-top: 16px;
    border-top: 1px solid var(--border-color);
}
.c-badge {
    padding: 4px 10px;
    border-radius: 6px;
    font-size: 10px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
</style>
`;
if (!content.includes('FORCE EL-DIALOG & EL-DRAWER Z-INDEX')) {
    content += '\n' + cssFix;
}

// 2. Thay the HTML
const htmlStart = '<div class="mb-6 contingency-plans-container" v-else-if="activityTab === \'contingency\'">';
const htmlStartIdx = content.indexOf(htmlStart);
const htmlEnd = '<div v-else-if="activityTab !== \'contingency\'" class="activity-empty-state">';
const htmlEndIdx = content.indexOf(htmlEnd);

if (htmlStartIdx !== -1 && htmlEndIdx !== -1) {
    const realStart = content.lastIndexOf('<div ', htmlStartIdx);
    
    const newHtml = `
            <div class="mb-6 contingency-plans-container" v-else-if="activityTab === 'contingency'">
               <div v-if="isLoadingContingency" class="p-8 text-center text-[var(--color-text-muted)]">
                 <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
                 <div class="mt-2 text-sm font-medium">Đang tải kế hoạch...</div>
               </div>
               <div v-else>
                  <div class="flex justify-between items-center mb-5">
                     <h3 class="sp-section-title mb-0 flex items-center text-[15px] font-bold"><i class="fa-solid fa-shield-halved text-[var(--color-warning)] mr-2 text-lg"></i> Kế hoạch dự phòng</h3>
                     <button class="s-btn s-btn-primary text-sm py-1.5 px-4 shadow-sm rounded-md font-semibold flex items-center transition-transform active:scale-95" @click="openCreateContingencyForm">
                        <i class="fa-solid fa-plus mr-1.5"></i> Thêm kế hoạch
                     </button>
                  </div>

                  <div class="space-y-4" v-if="contingencyPlans.length > 0">
                     <div v-for="plan in contingencyPlans" :key="plan.id" class="contingency-card">
                        <div class="contingency-card-header">
                           <div class="flex flex-col gap-2">
                              <h4 class="font-bold text-[16px] text-[var(--color-text-primary)] flex items-center">
                                 <i class="fa-solid fa-shield-halved text-[var(--color-accent)] mr-2 text-sm"></i>
                                 {{ plan.name }}
                              </h4>
                              <div class="flex flex-wrap items-center gap-2 mt-1">
                                 <span class="c-badge" :class="getRiskLevelClass(plan.riskLevel)">{{ plan.riskLevel }}</span>
                                 <span class="c-badge" :class="getRiskStatusClass(plan.riskStatus)">{{ plan.riskStatus }}</span>
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

                        <div class="contingency-trigger-box">
                           <div class="text-[11px] font-bold text-[var(--color-text-muted)] uppercase tracking-wider mb-1.5 flex items-center"><i class="fa-solid fa-bolt mr-1.5 text-[var(--color-warning)]"></i> Trigger (Điều kiện)</div>
                           <div class="text-[13px] text-[var(--color-text-primary)] font-medium leading-relaxed">
                              {{ plan.activationCondition }}
                           </div>
                        </div>

                        <div class="contingency-task-box" v-if="plan.contingencyTask">
                           <div class="text-[11px] font-bold text-[var(--color-accent)] uppercase tracking-wider mb-2 flex items-center"><i class="fa-solid fa-link mr-1.5"></i> Task dự phòng (Fallback)</div> 
                           <div class="cursor-pointer hover:underline text-[var(--color-text-primary)] font-semibold flex items-start text-[14px]" @click="openTaskDetail(plan.contingencyTask)">
                              <i class="fa-solid fa-square-check mt-0.5 mr-2 text-[var(--color-accent)] text-base"></i>
                              <span>{{ plan.contingencyTask.title }}</span>
                           </div>
                        </div>

                        <div class="contingency-footer" v-if="!plan.isActivated">
                           <button class="text-[13px] font-bold px-5 py-2 rounded-lg text-[var(--color-text-primary)] bg-[var(--bg-secondary)] border border-[var(--border-color)] hover:bg-[var(--border-color)] transition-all flex items-center" @click="openContingencyDetail(plan)">
                              <i class="fa-regular fa-file-lines mr-1.5 text-lg opacity-70"></i> Chi tiết
                           </button>
                           <button class="text-[13px] font-bold px-5 py-2 rounded-lg bg-[var(--color-accent)] text-white hover:opacity-90 hover:shadow-lg transition-all flex items-center shadow-md" @click="confirmActivateContingencyPlan(plan)">
                              <i class="fa-solid fa-rocket mr-2 text-lg"></i> Kích hoạt
                           </button>
                        </div>
                        
                        <div v-else-if="plan.activatedBy" style="margin-top: 16px; padding: 16px; background: rgba(34, 197, 94, 0.1); border-radius: 8px; border: 1px solid rgba(34, 197, 94, 0.3); display: flex; align-items: center; justify-content: space-between;">
                           <div class="flex items-center gap-3">
                              <div style="width: 36px; height: 36px; border-radius: 50%; background: #dcfce7; color: #166534; display: flex; align-items: center; justify-content: center; font-size: 18px;">
                                <i class="fa-solid fa-check"></i>
                              </div>
                              <div>
                                 <div style="font-size: 14px; font-weight: bold; color: #15803d;">Đã kích hoạt</div>
                                 <div style="font-size: 11px; color: var(--color-text-muted); margin-top: 2px;">Lúc {{ new Date(plan.activatedAt).toLocaleString() }}</div>
                              </div>
                           </div>
                           <div style="text-align: right;">
                              <div style="font-size: 11px; color: var(--color-text-muted); font-weight: 500;">Người thao tác</div>
                              <div style="font-size: 13px; font-weight: bold; color: var(--color-text-primary);">{{ plan.activatedBy.fullName || plan.activatedBy.email }}</div>
                           </div>
                        </div>
                     </div>
                  </div>
                  
                  <div v-else style="text-align: center; padding: 60px 20px; border: 2px dashed rgba(100,116,139,0.3); border-radius: 16px; background: var(--bg-secondary); cursor: pointer;" @click="openCreateContingencyForm">
                     <div style="width: 64px; height: 64px; border-radius: 50%; background: var(--bg-primary); display: flex; align-items: center; justify-content: center; margin: 0 auto 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.05);">
                        <i class="fa-solid fa-shield-halved" style="font-size: 32px; color: var(--color-text-muted);"></i>
                     </div>
                     <h4 style="color: var(--color-text-primary); font-size: 18px; font-weight: bold; margin-bottom: 8px;">Chưa có kế hoạch dự phòng</h4>
                     <p style="font-size: 13px; color: var(--color-text-muted); margin-bottom: 24px;">Một kế hoạch dự phòng giúp nhóm chuẩn bị sẵn các phương án fallback tự động khi công việc gặp rủi ro.</p>
                     <button class="s-btn s-btn-primary" style="padding: 10px 24px; border-radius: 8px; font-weight: bold; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);">
                        <i class="fa-solid fa-plus mr-2"></i> Tạo kế hoạch đầu tiên
                     </button>
                  </div>
               </div>
            </div>`;
    
    content = content.substring(0, realStart) + newHtml + '\n            ' + content.substring(htmlEndIdx);
    console.log('Replaced HTML tab container with guaranteed styles.');
} else {
    console.log('Failed to find htmlStartIdx or htmlEndIdx');
}

fs.writeFileSync(file, content, 'utf8');
