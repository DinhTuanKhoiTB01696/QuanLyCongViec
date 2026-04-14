<template>
  <AdminLayout>
    <div class="admin-page-container">
      <div class="page-header">
         <div class="breadcrumb">
           <i class="fa-solid fa-building"></i> Admin / Organization
         </div>
         <h1 class="page-title">Hồ sơ Tổ chức & Bảo mật (Tenant Settings)</h1>
         <p class="page-subtitle">Quản lý cấu hình toàn tổ chức, IP Whitelist và chính sách 2FA.</p>
      </div>

      <div class="admin-form-card" v-loading="isLoading">
         <el-tabs v-model="activeTab" class="custom-tabs">
            
            <!-- TAB PROFILE -->
            <el-tab-pane label="Thông tin Tổ chức" name="profile">
               <div class="form-group mt-12">
                  <label>Tên Tổ chức (Organization Name)</label>
                  <el-input v-model="form.organizationName" class="neumorphic-input" />
               </div>
               <div class="form-group">
                  <label>Tên miền (Domain)</label>
                  <el-input v-model="form.domain" class="neumorphic-input" />
               </div>
               <div class="form-group">
                  <label>URL Logo</label>
                  <el-input v-model="form.logoUrl" class="neumorphic-input" />
               </div>

               <div class="form-actions mt-24">
                  <el-button type="primary" :loading="isSaving" @click="saveConfig">
                     <i class="fa-solid fa-save mr-2"></i> Lưu thay đổi
                  </el-button>
               </div>
            </el-tab-pane>

            <!-- TAB SECURITY & IP WHITELIST -->
            <el-tab-pane label="Bảo vệ Hệ thống (Security)" name="security">
               <div style="margin-top: 16px; margin-bottom: 24px;">
                  <h3 style="font-size: 15px; margin-bottom: 8px; color: var(--text-primary)"><i class="fa-solid fa-shield-halved mr-2 text-warning"></i> Chính sách Bảo mật Toàn cầu</h3>
                  <p style="font-size: 13px; color: var(--text-secondary);">Thiết lập bắt buộc đối với tất cả thành viên trong tổ chức.</p>
               </div>

               <div class="form-group switch-group">
                  <div>
                    <h4 style="margin: 0; color: var(--text-primary)">Bắt buộc 2FA (Two-Factor Auth)</h4>
                    <span style="font-size: 12px; color: var(--text-muted)">Yêu cầu tất cả nhân viên phải cài đặt Google Authenticator để đăng nhập. Các luồng đăng nhập không có 2FA sẽ bị chặn lại (403 Forbidden).</span>
                  </div>
                  <el-switch :model-value="form.require2FA" active-color="#10b981" inactive-color="#dc2626" @change="val => form.require2FA = val" />
               </div>

               <div style="margin-top: 32px; margin-bottom: 16px;">
                  <h3 style="font-size: 15px; margin-bottom: 8px; color: var(--text-primary)"><i class="fa-solid fa-network-wired mr-2 text-blue-500"></i> Ip Whitelist (Tường lửa Tầng Ứng dụng)</h3>
                  <p style="font-size: 13px; color: var(--text-secondary);">Giới hạn các địa chỉ IP được phép truy cập vào hệ thống. Nhập theo định dạng dãy IP phân cách bởi dấu phẩy.</p>
               </div>

               <div class="form-group">
                  <label>Danh sách IP hợp lệ (IP Whitelist)</label>
                  <el-input v-model="form.ipWhitelist" type="textarea" :rows="4" class="neumorphic-input" placeholder="Ví dụ: 192.168.1.1, 203.0.113.50, [Lưu ý: Để trống = Mở cho mọi IP]" />
               </div>

               <div class="form-actions mt-24">
                  <el-button type="primary" :loading="isSaving" @click="saveConfig">
                     <i class="fa-solid fa-shield-cat mr-2"></i> Lưu cấu hình Bảo mật
                  </el-button>
               </div>
            </el-tab-pane>
         </el-tabs>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'

const activeTab = ref('profile')
const isLoading = ref(false)
const isSaving = ref(false)

const form = ref({
   organizationName: '',
   domain: '',
   logoUrl: '',
   require2FA: false,
   ipWhitelist: ''
})

onMounted(async () => {
   isLoading.value = true;
   try {
     const res = await axiosClient.get('/settings/Organization');
     const data = res.data.data || {};
     form.value.organizationName = data.organizationName || 'Global Organization';
     form.value.domain = data.domain || 'acme.com';
     form.value.logoUrl = data.logoUrl || '';
     form.value.require2FA = data.require2FA === 'true';
     form.value.ipWhitelist = data.ipWhitelist || '';
   } catch (err) {
     console.error(err);
     ElMessage.error('Không thể tải cấu hình Tenant.');
   } finally {
     isLoading.value = false;
   }
})

const saveConfig = async () => {
  try {
     isSaving.value = true;
     const payload = {
       Settings: {
         organizationName: form.value.organizationName,
         domain: form.value.domain,
         logoUrl: form.value.logoUrl,
         require2FA: form.value.require2FA ? 'true' : 'false',
         ipWhitelist: form.value.ipWhitelist
       }
     };
     await axiosClient.put('/settings/Organization', payload);
     ElMessage.success('Đã lưu cấu hình Tenant & Bảo mật thành công!');
  } catch (err) {
     console.error(err);
     ElMessage.error('Lỗi khi lưu cấu hình.');
  } finally {
     isSaving.value = false;
  }
}
</script>

<style scoped>
.admin-page-container {
  max-width: 900px;
}
.page-header {
  margin-bottom: 24px;
}
.breadcrumb {
  font-size: 13px;
  color: var(--text-muted);
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.page-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}
.page-subtitle {
  font-size: 14px;
  color: var(--text-muted);
  margin-top: 4px;
}
.admin-form-card {
  background: var(--bg-card);
  backdrop-filter: blur(var(--glass-blur, 12px));
  -webkit-backdrop-filter: blur(var(--glass-blur, 12px));
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-xl, 12px);
  padding: 32px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.1);
}
.form-group {
  margin-bottom: 20px;
}
.switch-group {
  display: flex; 
  align-items: center; 
  justify-content: space-between; 
  padding: 16px; 
  border: 1px solid var(--border-color); 
  border-radius: 8px;
  background: rgba(0, 0, 0, 0.2);
}
.form-group label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
  margin-bottom: 8px;
}
.mt-12 { margin-top: 12px; }
.mt-24 { margin-top: 24px; }
.mr-2 { margin-right: 8px; }
.text-warning { color: #f59e0b; }
.text-blue-500 { color: #3b82f6; }

:deep(.neumorphic-input .el-input__wrapper),
:deep(.neumorphic-input .el-textarea__inner) {
  background-color: var(--bg-hover) !important;
  box-shadow: none !important;
  border-radius: 8px;
  border: 1px solid var(--border-color);
}
:deep(.neumorphic-input .el-input__wrapper) { padding: 8px 16px; }
:deep(.neumorphic-input .el-textarea__inner) { padding: 12px 16px; font-family: inherit; color: var(--text-primary) !important; }
:deep(.neumorphic-input .el-input__inner) { color: var(--text-primary) !important; }
.form-actions { display: flex; justify-content: flex-end; }
</style>
