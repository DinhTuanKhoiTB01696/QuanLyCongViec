const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

// Fix the backticks in line 1233
content = content.replace(/background: \\`color-mix\(in srgb, \\\$\{(.*?)\} 15%, transparent\)\\`/g, "background: `color-mix(in srgb, ${$1} 15%, transparent)`");

// Fix the plan risk level part
const oldPlanHeader = `                              <div class="flex flex-wrap items-center gap-2 mt-1">
                                 <span class="text-[10px] px-2.5 py-1 rounded-md font-bold tracking-wide uppercase border shadow-sm" :class="getRiskLevelClass(plan.riskLevel)">{{ plan.riskLevel }}</span>
                              </div>`;

const newPlanHeader = `                              <div class="flex flex-wrap items-center gap-2 mt-1">
                                 <div class="subtask-chip" style="cursor: default;" :style="{
                                       color: plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6',
                                       borderColor: plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6',
                                       background: \`color-mix(in srgb, \${plan.riskLevel === 'Critical' ? '#ef4444' : plan.riskLevel === 'High' ? '#f97316' : plan.riskLevel === 'Medium' ? '#eab308' : '#3b82f6'} 15%, transparent)\`
                                    }">
                                    <i :class="plan.riskLevel === 'Critical' ? 'fa-solid fa-circle-exclamation' : plan.riskLevel === 'High' ? 'fa-solid fa-chevron-up' : plan.riskLevel === 'Medium' ? 'fa-solid fa-minus' : 'fa-solid fa-arrow-down'"></i>
                                    <span style="margin-left: 4px; font-weight: bold; text-transform: uppercase;">{{ plan.riskLevel || 'LOW' }}</span>
                                 </div>
                              </div>`;

content = content.replace(oldPlanHeader, newPlanHeader);

// We should also replace the container class to match the task card
const oldContainer = `<div class="contingency-card group hover:border-[var(--color-accent)] transition-all duration-300 relative overflow-hidden bg-[var(--bg-secondary)] border border-[var(--border-color)] rounded-xl p-5" v-for="plan in contingencyPlans" :key="plan.id">`;
const newContainer = `<div class="subtask-list p-4 rounded-lg border border-[var(--color-border)] bg-[var(--bg-secondary)]" style="display: flex; flex-direction: column; gap: 12px; margin: 0; box-shadow: var(--shadow-sm);" v-for="plan in contingencyPlans" :key="plan.id">`;
content = content.replace(oldContainer, newContainer);

// Change the button class for dropdown
const oldDropdownBtn = `<button class="text-[var(--color-text-muted)] hover:bg-[var(--bg-tertiary)] hover:text-[var(--color-text-primary)] w-8 h-8 rounded-lg flex items-center justify-center transition-all bg-transparent">`;
const newDropdownBtn = `<button class="nav-icon-btn bg-transparent border-0" type="button">`;
content = content.replace(oldDropdownBtn, newDropdownBtn);

// Change the dropdown menu items
const oldDropdownMenu = `<el-dropdown-menu class="theme-dropdown shadow-lg rounded-xl overflow-hidden border border-[var(--border-color)]">
                                       <el-dropdown-item command="edit" class="py-2.5 px-4 font-medium"><i class="fa-solid fa-pen mr-2 text-[var(--color-text-muted)]"></i> Chỉnh sửa</el-dropdown-item>
                                       <el-dropdown-item command="delete" class="py-2.5 px-4 font-medium text-red-600 hover:bg-red-50"><i class="fa-solid fa-trash mr-2"></i> Xóa kế hoạch</el-dropdown-item>
                                    </el-dropdown-menu>`;
const newDropdownMenu = `<el-dropdown-menu class="theme-dropdown">
                                       <el-dropdown-item command="edit"><i class="fa-solid fa-pen mr-2 text-[var(--color-text-muted)]"></i> Chỉnh sửa</el-dropdown-item>
                                       <el-dropdown-item command="delete" class="text-red-500"><i class="fa-solid fa-trash mr-2"></i> Xóa kế hoạch</el-dropdown-item>
                                    </el-dropdown-menu>`;
content = content.replace(oldDropdownMenu, newDropdownMenu);


fs.writeFileSync(file, content, 'utf8');
console.log("Replaced successfully!");
