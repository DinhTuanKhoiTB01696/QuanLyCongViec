<script setup>
import { ref } from 'vue'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'

const props = defineProps({
  projectId: { type: String, required: true },
  projectMembers: { type: Array, default: () => [] },
  projectStatuses: { type: Array, default: () => [] }
})

const emit = defineEmits(['imported'])

const showImportDialog = ref(false)
const previewRows = ref([])
const validRows = ref([])
const invalidRows = ref([])
const csvFileInput = ref(null)

function openImportDialog() {
  previewRows.value = []
  validRows.value = []
  invalidRows.value = []
  showImportDialog.value = true
}

function downloadTemplate() {
  const headers = "Tiêu đề công việc,Mô tả,Trạng thái,Người phụ trách email,Ưu tiên,Story Points,Hạn hoàn thành\n";
  const sampleRow = "Thiết kế giao diện dashboard,Thiết kế màn hình thống kê,TO DO,member@gmail.com,Cao,5,2026-07-31\n";
  
  // UTF-8 BOM
  const blob = new Blob([new Uint8Array([0xEF, 0xBB, 0xBF]), headers + sampleRow], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.setAttribute("download", "SprintA_Task_Template.csv");
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  ElMessage.success('Tải template mẫu thành công.');
}

function parseCSV(text) {
  const lines = [];
  let row = [""];
  let inQuotes = false;
  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    const next = text[i+1];
    if (c === '"') {
      if (inQuotes && next === '"') {
        row[row.length - 1] += '"';
        i++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (c === ',' && !inQuotes) {
      row.push("");
    } else if ((c === '\r' || c === '\n') && !inQuotes) {
      if (c === '\r' && next === '\n') {
        i++;
      }
      lines.push(row);
      row = [""];
    } else {
      row[row.length - 1] += c;
    }
  }
  if (row.length > 1 || row[0] !== "") {
    lines.push(row);
  }
  return lines;
}

function handleFileChange(event) {
  const file = event.target.files[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = (e) => {
    const text = e.target.result;
    const rawLines = parseCSV(text);
    if (rawLines.length <= 1) {
      ElMessage.warning('File CSV trống hoặc không đúng cấu trúc.');
      return;
    }

    const rowsToValidate = rawLines.slice(1);
    const parsed = [];
    const valid = [];
    const invalid = [];

    // Cache values for fast validation
    const emails = new Set((props.projectMembers || []).map(m => m.email?.toLowerCase().trim()).filter(Boolean));
    const statuses = new Set((props.projectStatuses || []).map(s => s.name?.toUpperCase().trim()).filter(Boolean));

    rowsToValidate.forEach((cols, idx) => {
      if (cols.length < 1 || !cols[0]?.trim()) {
        return;
      }

      const rowNum = idx + 2;
      const title = cols[0]?.trim() || '';
      const description = cols[1]?.trim() || '';
      const statusRaw = cols[2]?.trim() || '';
      const assigneeEmail = cols[3]?.trim() || '';
      const priority = cols[4]?.trim() || '';
      const storyPoints = cols[5]?.trim() || '';
      const dueDate = cols[6]?.trim() || '';

      const record = { rowNum, title, description, status: statusRaw, assigneeEmail, priority, storyPoints, dueDate };

      let errorMsg = '';
      if (!title) {
        errorMsg = 'Thiếu tiêu đề công việc';
      } else if (assigneeEmail && !emails.has(assigneeEmail.toLowerCase())) {
        errorMsg = 'Email người phụ trách không thuộc dự án';
      } else if (statusRaw && !statuses.has(statusRaw.toUpperCase())) {
        errorMsg = 'Trạng thái công việc không tồn tại trong dự án';
      } else if (dueDate && isNaN(Date.parse(dueDate))) {
        errorMsg = 'Hạn hoàn thành không đúng định dạng ngày (VD: YYYY-MM-DD)';
      }

      if (errorMsg) {
        record.error = errorMsg;
        invalid.push(record);
      } else {
        valid.push(record);
      }
      parsed.push(record);
    });

    previewRows.value = parsed;
    validRows.value = valid;
    invalidRows.value = invalid;

    if (csvFileInput.value) csvFileInput.value.value = '';
  };
  reader.readAsText(file, 'UTF-8');
}

async function confirmImport() {
  if (!validRows.value.length) return;
  try {
    await axiosClient.post(`/projects/${props.projectId}/WorkTasks/import`, validRows.value);
    ElMessage.success(`Nhập thành công ${validRows.value.length} công việc.`);
    showImportDialog.value = false;
    emit('imported');
  } catch (e) {
    ElMessage.error(e.response?.data?.message || 'Lỗi khi nhập công việc.');
  }
}

async function exportTasks() {
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks/export`, { responseType: 'blob' });
    const blob = new Blob([res.data], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.setAttribute("download", `SprintA-Tasks-${props.projectId}-${new Date().getTime()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    ElMessage.success('Xuất dữ liệu thành công.');
  } catch (e) {
    ElMessage.error('Không thể xuất dữ liệu công việc.');
  }
}
</script>

<template>
  <div class="task-import-export-wrapper" style="display: inline-flex; align-items: center; margin-right: 12px;">
    <el-button-group>
      <el-button type="info" plain size="default" @click="openImportDialog">
        <i class="fa-solid fa-file-import mr-1"></i> Nhập Excel/CSV
      </el-button>
      <el-button type="info" plain size="default" @click="exportTasks">
        <i class="fa-solid fa-file-export mr-1"></i> Xuất Excel/CSV
      </el-button>
    </el-button-group>

    <!-- Dialog Import Excel/CSV -->
    <el-dialog v-model="showImportDialog" title="Nhập danh sách công việc từ Excel/CSV" width="840px" destroy-on-close append-to-body>
      <div class="import-dialog-content">
        <div class="import-help mb-4 flex justify-between items-center" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
          <span class="text-xs text-[var(--color-text-muted)]" style="font-size: 12px; color: var(--color-text-muted);">Tải file template mẫu, điền thông tin công việc rồi tải lên hệ thống.</span>
          <el-button type="primary" size="small" @click="downloadTemplate">
            <i class="fa-solid fa-download mr-1"></i> Tải template mẫu (.csv)
          </el-button>
        </div>

        <div class="upload-zone p-6 border-2 border-dashed border-[var(--color-border)] rounded-xl text-center bg-gray-50/50 hover:bg-gray-50 transition-colors mb-4" style="padding: 24px; border: 2px dashed var(--color-border); border-radius: 12px; text-align: center; margin-bottom: 16px; background: rgba(248, 250, 252, 0.5);">
          <input type="file" ref="csvFileInput" class="hidden" accept=".csv" @change="handleFileChange" id="csv-file-upload-input" style="display: none;" />
          <label for="csv-file-upload-input" class="cursor-pointer block" style="cursor: pointer; display: block;">
            <i class="fa-solid fa-cloud-arrow-up text-3xl text-[var(--color-text-muted)] mb-2" style="font-size: 28px; color: var(--color-text-muted); margin-bottom: 8px;"></i>
            <p class="text-sm font-bold text-[var(--color-text-primary)]" style="font-size: 14px; font-weight: bold; color: var(--color-text-primary); margin: 0;">Chọn file CSV chứa danh sách công việc</p>
            <p class="text-xs text-[var(--color-text-muted)] mt-1" style="font-size: 11px; color: var(--color-text-muted); margin-top: 4px;">Hỗ trợ file định dạng CSV (UTF-8) hoặc các file Excel Save As thành CSV.</p>
          </label>
        </div>

        <!-- Preview Area -->
        <div v-if="previewRows.length" class="preview-area mt-4" style="margin-top: 16px;">
          <div class="flex justify-between items-center mb-2" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
            <h4 class="text-sm font-bold text-[var(--color-text-primary)]" style="font-size: 13px; font-weight: bold; margin: 0;">Kết quả phân tích file ({{ previewRows.length }} dòng)</h4>
            <div class="flex gap-2" style="display: flex; gap: 8px;">
              <el-tag type="success" size="small">{{ validRows.length }} Hợp lệ</el-tag>
              <el-tag type="danger" size="small">{{ invalidRows.length }} Lỗi</el-tag>
            </div>
          </div>

          <el-tabs type="border-card" class="import-preview-tabs">
            <el-tab-pane label="Dòng hợp lệ">
              <div class="table-wrap max-h-60 overflow-y-auto" style="max-height: 240px; overflow-y: auto;">
                <table class="w-full text-xs text-left border-collapse" style="width: 100%; border-collapse: collapse; text-align: left; font-size: 12px;">
                  <thead>
                    <tr class="bg-gray-100 font-bold border-b border-[var(--color-border)]" style="background: rgba(148, 163, 184, 0.08); font-weight: bold; border-bottom: 1px solid var(--color-border);">
                      <th class="p-2" style="padding: 8px;">Tiêu đề</th>
                      <th class="p-2" style="padding: 8px;">Mô tả</th>
                      <th class="p-2" style="padding: 8px;">Trạng thái</th>
                      <th class="p-2" style="padding: 8px;">Người phụ trách</th>
                      <th class="p-2" style="padding: 8px;">Ưu tiên</th>
                      <th class="p-2" style="padding: 8px;">Story Points</th>
                      <th class="p-2" style="padding: 8px;">Hạn hoàn thành</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(row, idx) in validRows" :key="idx" class="border-b border-[var(--color-border)] hover:bg-gray-50/50" style="border-bottom: 1px solid var(--color-border);">
                      <td class="p-2 font-bold" style="padding: 8px; font-weight: bold;">{{ row.title }}</td>
                      <td class="p-2 truncate max-w-xs" style="padding: 8px; max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">{{ row.description }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.status }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.assigneeEmail }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.priority }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.storyPoints }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.dueDate }}</td>
                    </tr>
                    <tr v-if="!validRows.length">
                      <td colspan="7" class="p-4 text-center text-[var(--color-text-muted)]" style="padding: 16px; text-align: center; color: var(--color-text-muted);">Không có dòng hợp lệ nào để nhập.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </el-tab-pane>

            <el-tab-pane label="Dòng lỗi">
              <div class="table-wrap max-h-60 overflow-y-auto" style="max-height: 240px; overflow-y: auto;">
                <table class="w-full text-xs text-left border-collapse" style="width: 100%; border-collapse: collapse; text-align: left; font-size: 12px;">
                  <thead>
                    <tr class="bg-gray-100 font-bold border-b border-[var(--color-border)]" style="background: rgba(148, 163, 184, 0.08); font-weight: bold; border-bottom: 1px solid var(--color-border);">
                      <th class="p-2 text-red-500" style="padding: 8px; color: #ef4444;">Lý do lỗi</th>
                      <th class="p-2" style="padding: 8px;">Dòng</th>
                      <th class="p-2" style="padding: 8px;">Tiêu đề</th>
                      <th class="p-2" style="padding: 8px;">Trạng thái</th>
                      <th class="p-2" style="padding: 8px;">Người phụ trách</th>
                      <th class="p-2" style="padding: 8px;">Hạn hoàn thành</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(row, idx) in invalidRows" :key="idx" class="border-b border-[var(--color-border)] bg-red-50/30 hover:bg-red-50/50" style="border-bottom: 1px solid var(--color-border); background: rgba(239, 68, 68, 0.02);">
                      <td class="p-2 text-red-500 font-semibold" style="padding: 8px; color: #ef4444; font-weight: 600;">{{ row.error }}</td>
                      <td class="p-2 text-[var(--color-text-muted)]" style="padding: 8px; color: var(--color-text-muted);">#{{ row.rowNum }}</td>
                      <td class="p-2 font-bold" style="padding: 8px; font-weight: bold;">{{ row.title || '—' }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.status || '—' }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.assigneeEmail || '—' }}</td>
                      <td class="p-2" style="padding: 8px;">{{ row.dueDate || '—' }}</td>
                    </tr>
                    <tr v-if="!invalidRows.length">
                      <td colspan="6" class="p-4 text-center text-green-600" style="padding: 16px; text-align: center; color: #16a34a;">Tuyệt vời! Không có dòng nào bị lỗi.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </el-tab-pane>
          </el-tabs>
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer flex justify-between" style="display: flex; justify-content: space-between; align-items: center; width: 100%; padding-top: 12px; border-top: 1px solid var(--color-border);">
          <span class="text-xs text-[var(--color-text-muted)] flex items-center" style="font-size: 11px; color: var(--color-text-muted);">
            * Các dòng bị lỗi sẽ tự động bị bỏ qua khi nhập.
          </span>
          <div>
            <el-button @click="showImportDialog = false">Hủy bỏ</el-button>
            <el-button type="primary" :disabled="!validRows.length" @click="confirmImport">
              Xác nhận nhập ({{ validRows.length }} dòng)
            </el-button>
          </div>
        </div>
      </template>
    </el-dialog>
  </div>
</template>
