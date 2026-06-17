<template>
  <div class="goal-detail-wrapper" v-if="goal">
    <!-- Module Header (matching the list view) -->
    <header class="module-header">
      <div class="header-content">
        <h1>Mục tiêu</h1>
        <div class="header-actions">
          <button class="primary-btn">Tạo mục tiêu</button>
        </div>
      </div>
      
      <div class="tabs-nav">
        <router-link to="/home/goals" class="tab-link">Thư mục mục tiêu</router-link>
        <router-link to="/home/goals" class="tab-link">Đang theo dõi</router-link>
        <router-link to="/home/goals" class="tab-link">Đã lưu trữ</router-link>
      </div>
    </header>

    <!-- Entity Header -->
    <header class="goal-header">
      <div class="goal-header-inner">
        <!-- Entity Main info -->
        <div class="header-main">
          <div class="title-block">
            <div class="goal-icon-large">
              <i class="fa-solid fa-bullseye"></i>
            </div>
            <h1>{{ goal.title }}</h1>
          </div>
          
          <div class="header-actions">
            <button class="secondary-btn" @click="toggleFollow">
              <i class="fa-solid fa-eye"></i> {{ goal.isFollowing ? 'Đang theo dõi' : 'Theo dõi' }}
            </button>
            <button class="secondary-btn icon-only" @click="toggleStar" :class="{ starred: goal.isStarred }">
              <i :class="goal.isStarred ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i>
            </button>
            <button class="secondary-btn icon-only" @click="toggleShare">
              <i class="fa-solid fa-share-nodes"></i>
            </button>
            <button class="secondary-btn icon-only" @click="toggleMenu">
              <i class="fa-solid fa-ellipsis"></i>
            </button>
          </div>
        </div>
        
        <!-- Quick Status Row -->
        <div class="quick-status-row">
          <span class="status-badge" :class="getStatusClass(goal.status)">
            <span class="status-dot"></span> {{ goal.status }}
          </span>
          <span class="update-text" v-if="goal.lastUpdate">Cập nhật: {{ goal.lastUpdate }}</span>
        </div>
      </div>
    </header>

    <!-- Navigation Tabs -->
    <div style="border-bottom: 2px solid #DFE1E6; background: white;">
      <div class="goal-tabs-nav" style="padding: 0 40px; display: flex; gap: 24px; max-width: 1000px; margin: 0 auto; width: 100%;">
        <button class="tab-btn" :class="{ active: currentTab === 'overview' }" @click="currentTab = 'overview'">Tổng quan</button>
        <button class="tab-btn" :class="{ active: currentTab === 'updates' }" @click="currentTab = 'updates'">Cập nhật <span v-if="updates.length" class="badge-count">{{ updates.length + 1 }}</span></button>
        <button class="tab-btn" :class="{ active: currentTab === 'jira' }" @click="currentTab = 'jira'">Jira</button>
        <button class="tab-btn" :class="{ active: currentTab === 'projects' }" @click="currentTab = 'projects'">Dự án</button>
        <button class="tab-btn" :class="{ active: currentTab === 'learnings' }" @click="currentTab = 'learnings'">Bài học rút ra</button>
        <button class="tab-btn" :class="{ active: currentTab === 'risks' }" @click="currentTab = 'risks'">Rủi ro</button>
        <button class="tab-btn" :class="{ active: currentTab === 'decisions' }" @click="currentTab = 'decisions'">Quyết định</button>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="goal-content-grid">
      <!-- Left Column: Main Information -->
      <div class="main-column">
        <!-- TỔNG QUAN TAB -->
        <template v-if="currentTab === 'overview'">
          <!-- Mô tả -->
          <section class="content-section">
            <div class="section-header">
              <h3>Mô tả</h3>
            </div>
            <div class="section-body">
              <RichTextEditor v-if="isEditingBio" v-model="tempBio" @save="saveBio" @cancel="isEditingBio = false" placeholder="Mô tả ngắn gọn lý do tại sao mục tiêu này lại quan trọng và cách đo lường thành công..." />
              <div v-else @click="startEditingBio" style="cursor: pointer; color: #5E6C84; font-size: 14px; padding: 8px; border-radius: 3px; min-height: 40px;" onmouseover="this.style.backgroundColor='#FAFBFC'" onmouseout="this.style.backgroundColor='transparent'">
                {{ goal.description || 'Mô tả ngắn gọn lý do tại sao mục tiêu này lại quan trọng và cách đo lường thành công, để bạn có thể cung cấp hiểu biết chung cho người theo dõi.' }}
              </div>
            </div>
          </section>

          <!-- Key results -->
          <section class="content-section">
            <div class="section-header">
              <h3>Key results</h3>
            </div>
            <div class="section-body">
              <div style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 24px; display: flex; justify-content: space-between; align-items: center; background: white;">
                <div style="max-width: 400px;">
                  <h4 style="margin: 0 0 8px 0; font-size: 14px; font-weight: 600; color: #172B4D;">Theo dõi tiến độ hướng tới mục tiêu của bạn</h4>
                  <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Chia sẻ nội dung cập nhật riêng cho từng kết quả chính để tự động cập nhật tiến độ của mục tiêu này.</p>
                  <button class="primary-btn" style="display: inline-flex; align-items: center; gap: 8px; height: 32px;"><i class="fa-solid fa-plus"></i> Tạo</button>
                </div>
                <div>
                  <div style="width: 180px; height: 100px; background: #E6FCFF; border-radius: 8px; display: flex; align-items: center; justify-content: center; position: relative; overflow: hidden;">
                    <div style="position: absolute; right: -20px; top: -20px; width: 80px; height: 80px; background: #0052CC; transform: rotate(45deg);"></div>
                    <i class="fa-solid fa-chart-line" style="font-size: 40px; color: #0052CC; z-index: 1;"></i>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <!-- Hoạt động -->
          <section class="content-section" style="margin-top: 16px;">
            <div class="section-header" style="border: none; padding-bottom: 0;">
              <h3>Hoạt động</h3>
            </div>
            <div style="display: flex; gap: 8px; margin-bottom: 16px;">
              <button class="toggle-btn" :class="{ active: activityTab === 'comments' }" @click="activityTab = 'comments'">Nhận xét</button>
              <button class="toggle-btn" :class="{ active: activityTab === 'history' }" @click="activityTab = 'history'">Lịch sử</button>
            </div>
            
            <div class="section-body">
              <div v-if="activityTab === 'comments'">
                <h4 style="font-size: 14px; color: #172B4D; margin: 0 0 16px 0;">Comments</h4>
                <div class="update-input-mockup" style="background: #FAFBFC; padding: 12px; border-radius: 3px; border: 1px solid transparent; transition: border 0.2s;" onmouseover="this.style.border='1px solid #DFE1E6'" onmouseout="this.style.border='1px solid transparent'">
                  <div class="user-avatar-current" style="background: #36B37E;">T</div>
                  <input type="text" placeholder="Thêm nhận xét... đặt câu hỏi" style="border: none; background: transparent; width: 100%; outline: none; font-size: 14px;" />
                </div>
              </div>
              <div v-else>
                <div class="timeline-item" style="display: flex; align-items: flex-start; gap: 12px;">
                   <div class="user-avatar-current" style="background: #36B37E; width: 24px; height: 24px; font-size: 10px;">T</div>
                   <div style="flex: 1;">
                      <div style="display: flex; justify-content: space-between; align-items: center;">
                         <span style="font-size: 14px; color: #172B4D;"><strong>Tua20000</strong> đã tạo Mục tiêu</span>
                         <span style="font-size: 12px; color: #5E6C84;">9 days ago</span>
                      </div>
                      <div style="font-size: 14px; color: #5E6C84; margin-top: 4px;">{{ goal.title }}</div>
                   </div>
                </div>
              </div>
            </div>
          </section>
        </template>

        <!-- CẬP NHẬT TAB -->
        <template v-if="currentTab === 'updates'">
           <div class="status-update-box">
             <div class="update-input-mockup" @click="showUpdateForm = true" v-if="!showUpdateForm" style="background: white; border: 1px solid #DFE1E6; padding: 16px; border-radius: 3px;">
               <div class="user-avatar-current" style="background: #36B37E;">T</div>
               <div style="color: #5E6C84; font-size: 14px;">Đăng bản cập nhật của bạn</div>
             </div>
             
             <div class="update-form-active" v-else style="background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 8px -2px rgba(9,30,66,0.25); overflow: hidden;">
               <!-- Status Dropdown & Date row -->
               <div style="display: flex; justify-content: space-between; align-items: center; padding: 16px; border-bottom: 1px solid #DFE1E6; background: #FAFBFC;">
                 <div style="position: relative;">
                   <label style="font-size: 11px; font-weight: 600; color: #6B778C; display: block; margin-bottom: 4px;">Trạng thái hiện tại</label>
                   <div @click="isUpdateStatusOpen = !isUpdateStatusOpen" style="display: flex; align-items: center; gap: 4px; cursor: pointer;">
                      <span class="status-badge" :class="getStatusClass(newUpdate.status)" style="font-size: 11px; padding: 4px 8px;">{{ newUpdate.status }} <i class="fa-solid fa-chevron-down" style="font-size: 10px; margin-left: 4px;"></i></span>
                   </div>
                   <!-- Status Menu -->
                   <div v-if="isUpdateStatusOpen" class="dropdown-menu" style="position: absolute; top: 100%; left: 0; margin-top: 4px; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); width: 200px; z-index: 100; display: flex; flex-direction: column; padding: 8px 0;">
                      <div v-for="st in ['ĐANG CHỜ XỬ LÝ', 'ĐÚNG TIẾN ĐỘ', 'CÓ RỦI RO', 'KHÔNG ĐÚNG TIẾN ĐỘ', 'ĐÃ HOÀN TẤT', 'ĐÃ TẠM DỪNG', 'ĐÃ HỦY']" :key="st" @click="newUpdate.status = st; isUpdateStatusOpen = false" style="padding: 6px 12px; cursor: pointer; display: flex; align-items: center;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                         <span class="status-badge" :class="getStatusClass(st)" style="font-size: 10px;">{{ st }}</span>
                      </div>
                   </div>
                 </div>
                 
                 <div>
                   <label style="font-size: 11px; font-weight: 600; color: #6B778C; display: block; margin-bottom: 4px;">Ngày mục tiêu</label>
                   <div style="display: flex; align-items: center; gap: 4px; color: #172B4D; font-size: 13px; font-weight: 500;">
                     <i class="fa-regular fa-calendar" style="color: #6B778C;"></i> Tháng 07 <i class="fa-solid fa-chevron-down" style="font-size: 10px; color: #6B778C;"></i>
                   </div>
                 </div>
                 
                 <div>
                    <label style="font-size: 11px; font-weight: 600; color: #6B778C; display: block; margin-bottom: 4px;">Tiến độ</label>
                    <div style="font-size: 13px; color: #172B4D; display: flex; align-items: center; gap: 8px;">
                      <input type="number" v-model="newUpdate.progress" style="width: 50px; padding: 2px 4px; border: 1px solid #DFE1E6; border-radius: 3px; font-size: 13px;" /> % -> 100%
                    </div>
                 </div>
               </div>
               
               <div style="padding: 16px;">
                 <textarea v-model="newUpdate.message" rows="4" style="width: 100%; border: none; outline: none; font-size: 14px; color: #172B4D; resize: none; font-family: inherit;" placeholder="Viết bản cập nhật gồm tối đa 280 ký tự. Nhập '/gần nhất' để sao chép bản cập nhật gần đây nhất, còn nhập / để thêm thành phần khác" maxlength="280"></textarea>
                 
                 <div style="display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #DFE1E6; padding-top: 12px; margin-top: 8px;">
                   <div style="display: flex; gap: 16px; color: #6B778C; font-size: 16px;">
                     <i class="fa-regular fa-face-smile" style="cursor: pointer;"></i>
                     <i class="fa-regular fa-image" style="cursor: pointer;"></i>
                     <i class="fa-solid fa-link" style="cursor: pointer;"></i>
                   </div>
                   <div style="display: flex; align-items: center; gap: 12px;">
                     <span style="font-size: 12px; color: #5E6C84;">{{ newUpdate.message.length }}/280</span>
                     <button class="primary-btn" @click="postUpdate" :disabled="!newUpdate.message && !newUpdate.progress" style="width: 80px;">Đăng</button>
                   </div>
                 </div>
               </div>
             </div>
           </div>

           <!-- Latest update Mockup -->
           <div>
             <h4 style="font-size: 14px; font-weight: 600; color: #172B4D; margin: 0 0 16px 0; display: flex; align-items: center; gap: 8px;">Latest update <span style="font-size: 12px; font-weight: 400; color: #5E6C84;">Nội dung cập nhật đầu tiên trong chuỗi</span></h4>
             
             <div style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 16px; background: white; margin-bottom: 24px;">
               <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 12px;">
                 <div style="display: flex; gap: 12px;">
                    <div class="user-avatar-current" style="background: #36B37E; width: 32px; height: 32px;">T</div>
                    <div>
                      <div style="font-size: 14px; font-weight: 600; color: #172B4D;">Tua20000</div>
                      <div style="font-size: 12px; color: #5E6C84;">khoảng 12 giờ trước • 1 người đã xem</div>
                    </div>
                 </div>
                 <span class="status-badge" :class="getStatusClass('Đã hoàn tất')" style="font-size: 11px;">ĐÃ HOÀN TẤT</span>
               </div>
               
               <p style="font-size: 14px; color: #172B4D; margin: 0 0 16px 0;">Cập nhật xong chức năng mục tiêu.</p>
               
               <div style="background: #FAFBFC; border: 1px solid #DFE1E6; border-radius: 3px; padding: 12px; display: flex; align-items: center; gap: 16px; margin-bottom: 16px;">
                  <span style="font-size: 12px; color: #5E6C84;">Đã thay đổi trạng thái</span>
                  <div style="display: flex; align-items: center; gap: 8px;">
                     <span class="status-badge" style="font-size: 10px; background: #DFE1E6; color: #42526E;">ĐANG CHỜ XỬ LÝ</span>
                     <i class="fa-solid fa-arrow-right" style="font-size: 10px; color: #5E6C84;"></i>
                     <span class="status-badge" style="font-size: 10px; background: #EAE6FF; color: #403294;">ĐÃ HOÀN TẤT</span>
                  </div>
               </div>
               
               <div style="display: flex; align-items: center; gap: 12px; font-size: 13px; color: #5E6C84; border-bottom: 1px solid #DFE1E6; padding-bottom: 12px; margin-bottom: 12px;">
                 <span style="cursor: pointer;" onmouseover="this.style.textDecoration='underline'" onmouseout="this.style.textDecoration='none'">Chia sẻ</span> •
                 <span style="cursor: pointer;" onmouseover="this.style.textDecoration='underline'" onmouseout="this.style.textDecoration='none'">Sửa</span> •
                 <span style="cursor: pointer;" onmouseover="this.style.textDecoration='underline'" onmouseout="this.style.textDecoration='none'">Xóa</span> •
                 <div style="display: flex; gap: 4px;">
                   <span style="background: #FFF0B3; padding: 2px 6px; border-radius: 12px; cursor: pointer;">👍 1</span>
                   <span style="background: #FFEBE6; padding: 2px 6px; border-radius: 12px; cursor: pointer;">❤️ 1</span>
                   <span style="background: #FAFBFC; border: 1px solid #DFE1E6; padding: 2px 6px; border-radius: 12px; cursor: pointer;"><i class="fa-regular fa-face-smile"></i></span>
                 </div>
               </div>
               
               <div style="display: flex; align-items: center; gap: 12px;">
                 <div class="user-avatar-current" style="background: #36B37E; width: 24px; height: 24px; font-size: 10px;">T</div>
                 <input type="text" placeholder="Thêm nhận xét... ăn mừng cùng đồng đội của bạn" style="flex: 1; border: none; background: #FAFBFC; padding: 8px 12px; border-radius: 3px; font-size: 13px; outline: none;" />
               </div>
             </div>
           </div>
        </template>

        <!-- JIRA TAB -->
        <template v-if="currentTab === 'jira'">
          <div style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 32px; background: white; margin-top: 16px;">
            <div style="display: flex; gap: 24px; max-width: 600px; margin: 0 auto; position: relative;">
               <div style="width: 64px; height: 64px; background: #0052CC; color: white; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 32px; position: relative; flex-shrink: 0;">
                  <i class="fa-solid fa-layer-group"></i>
                  <div style="position: absolute; bottom: -8px; right: -8px; width: 24px; height: 24px; background: #0052CC; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 14px; border: 2px solid white;">
                     <i class="fa-solid fa-plus"></i>
                  </div>
               </div>
               <div>
                 <h3 style="margin: 0 0 8px 0; font-size: 16px; color: #172B4D;">Thêm công việc trong Jira góp phần vào mục tiêu này</h3>
                 <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Kết nối công việc của đội ngũ để xem mục tiêu này trong Jira và liên kết các nội dung cập nhật với công việc. <a href="#" style="color: #0052CC; text-decoration: none;">Thông tin khác về mục tiêu trong Jira</a></p>
                 <div style="position: relative; display: inline-block;">
                   <button class="secondary-btn" @click="isJiraInputOpen = !isJiraInputOpen" style="background: white; border: 1px solid #DFE1E6; font-weight: 600;">Thêm hạng mục công việc Jira</button>
                   
                   <!-- Jira Input Dropdown -->
                   <div v-if="isJiraInputOpen" class="dropdown-menu" style="position: absolute; top: 100%; left: 0; margin-top: 8px; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); width: 320px; z-index: 100; padding: 16px;">
                      <input type="text" placeholder="Dán URL Jira" style="width: 100%; padding: 8px 12px; border: 2px solid #4C9AFF; border-radius: 3px; font-size: 14px; outline: none; box-sizing: border-box; margin-bottom: 12px;" />
                      <div style="display: flex; justify-content: flex-end; gap: 8px;">
                        <button class="cancel-btn" @click="isJiraInputOpen = false">Hủy</button>
                        <button class="primary-btn">Thêm</button>
                      </div>
                   </div>
                 </div>
               </div>
            </div>
          </div>
        </template>

        <!-- DỰ ÁN TAB -->
        <template v-if="currentTab === 'projects'">
          <div style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 32px; background: white; margin-top: 16px;">
            <div style="display: flex; gap: 24px; max-width: 600px; margin: 0 auto; position: relative;">
               <div style="width: 64px; height: 64px; background: #EBECF0; color: #172B4D; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 32px; position: relative; flex-shrink: 0;">
                  <i class="fa-solid fa-rocket"></i>
                  <div style="position: absolute; bottom: -8px; right: -8px; width: 24px; height: 24px; background: #0052CC; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 14px; border: 2px solid white;">
                     <i class="fa-solid fa-plus"></i>
                  </div>
               </div>
               <div>
                 <h3 style="margin: 0 0 8px 0; font-size: 16px; color: #172B4D;">Thêm dự án để sắp xếp công việc của bạn với mục tiêu này</h3>
                 <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Sử dụng không gian này để theo dõi bất kỳ dự án nào đóng góp vào mục tiêu này, vì vậy đội ngũ và các bên liên quan của bạn có thể có được bức tranh toàn cảnh.</p>
                 <div style="position: relative; display: inline-block;">
                   <button class="secondary-btn" @click="isProjectSearchOpen = !isProjectSearchOpen" style="background: white; border: 1px solid #DFE1E6; font-weight: 600;">Thêm dự án</button>
                   
                   <!-- Project Search Dropdown -->
                   <div v-if="isProjectSearchOpen" class="dropdown-menu" style="position: absolute; top: 100%; left: 0; margin-top: 8px; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); width: 320px; z-index: 100; padding: 12px 0;">
                      <div style="padding: 0 12px 12px;">
                        <input type="text" placeholder="Tìm kiếm dự án" style="width: 100%; padding: 8px 12px; border: 2px solid #DFE1E6; border-radius: 3px; font-size: 14px; outline: none; box-sizing: border-box;" onfocus="this.style.borderColor='#4C9AFF'" onblur="this.style.borderColor='#DFE1E6'" />
                      </div>
                      <div style="max-height: 200px; overflow-y: auto;">
                        <div v-for="proj in ['uqe', 'e', 'ueq', 'rWÉ']" :key="proj" style="padding: 8px 12px; cursor: pointer; display: flex; align-items: center; gap: 8px; font-size: 14px; color: #172B4D;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                           <div style="width: 16px; height: 16px; background: #FFAB00; border-radius: 3px; font-size: 10px; display: flex; align-items: center; justify-content: center;"><i class="fa-solid fa-rocket" style="color: white;"></i></div>
                           {{ proj }}
                        </div>
                      </div>
                      <div style="padding: 12px 12px 0; border-top: 1px solid #DFE1E6; margin-top: 4px;">
                        <span style="font-size: 14px; color: #5E6C84; cursor: pointer;" onmouseover="this.style.textDecoration='underline'" onmouseout="this.style.textDecoration='none'">Tạo dự án mới</span>
                      </div>
                   </div>
                 </div>
               </div>
            </div>
          </div>
        </template>

        <!-- BÀI HỌC RÚT RA TAB -->
        <template v-if="currentTab === 'learnings'">
          <div v-if="!isEditingLearning" style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 40px; background: white; margin-top: 16px; display: flex; align-items: center; justify-content: center; gap: 32px;">
             <div style="position: relative;">
               <div style="width: 48px; height: 48px; background: #FFAB00; border-radius: 50%; position: relative; z-index: 1;"></div>
               <div style="position: absolute; bottom: -8px; left: 50%; transform: translateX(-50%); width: 24px; height: 16px; background: #172B4D; border-radius: 4px; z-index: 2;"></div>
               <div style="position: absolute; top: -16px; left: 50%; transform: translateX(-50%); color: #172B4D; font-size: 16px;">\ | /</div>
             </div>
             <div style="max-width: 400px;">
                <h3 style="margin: 0 0 8px 0; font-size: 14px; color: #172B4D; text-decoration: line-through;">Những bộ óc vĩ đại có tư duy giống nhau</h3>
                <h3 style="margin: 0 0 8px 0; font-size: 14px; color: #172B4D;">sẽ chia sẻ kiến thức của họ</h3>
                <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Chia sẻ bất kỳ bài học kinh nghiệm nào để giúp các đội ngũ khác có khởi đầu thuận lợi khi thực hiện các mục tiêu tương tự.</p>
                <div style="display: flex; gap: 16px; align-items: center;">
                  <button class="secondary-btn" @click="isEditingLearning = true" style="background: white; border: 1px solid #DFE1E6; font-weight: 600;">Thêm learning mới</button>
                  <a href="#" style="color: #5E6C84; text-decoration: none; font-size: 14px;">Xem ví dụ</a>
                </div>
             </div>
          </div>

          <div v-else style="border: 1px solid #4C9AFF; border-radius: 3px; background: white; margin-top: 16px; overflow: hidden; box-shadow: 0 0 0 1px #4C9AFF;">
             <div style="padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; align-items: center; gap: 12px;">
                <i class="fa-regular fa-lightbulb" style="color: #FFAB00; font-size: 18px;"></i>
                <input type="text" placeholder="Tóm tắt cho bài học rút ra của bạn là gì?" style="border: none; outline: none; font-size: 16px; color: #172B4D; width: 100%;" />
             </div>
             
             <!-- Rich Text Editor Toolbar Mockup -->
             <div style="display: flex; gap: 8px; padding: 8px 16px; border-bottom: 1px solid #DFE1E6; background: white; flex-wrap: wrap; align-items: center;">
                <div style="display: flex; align-items: center; gap: 4px; padding: 4px 8px; cursor: pointer; color: #5E6C84; border-radius: 3px;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                  <span style="font-size: 13px;">Normal text</span> <i class="fa-solid fa-chevron-down" style="font-size: 10px;"></i>
                </div>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-bold toolbar-icon"></i>
                <i class="fa-solid fa-italic toolbar-icon"></i>
                <i class="fa-solid fa-ellipsis toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <div style="display: flex; flex-direction: column; align-items: center; cursor: pointer; padding: 4px; border-radius: 3px;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                   <i class="fa-solid fa-font" style="font-size: 14px; color: #5E6C84;"></i>
                   <div style="width: 12px; height: 3px; background: #FF5630; margin-top: 2px;"></div>
                </div>
                <i class="fa-solid fa-chevron-down toolbar-icon" style="font-size: 10px; margin-left: -4px;"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-list-ul toolbar-icon"></i>
                <i class="fa-solid fa-list-ol toolbar-icon"></i>
                <i class="fa-regular fa-square-check toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-link toolbar-icon"></i>
                <i class="fa-solid fa-at toolbar-icon"></i>
                <i class="fa-regular fa-face-smile toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-table-cells toolbar-icon"></i>
                <div style="position: relative;">
                  <i class="fa-solid fa-columns toolbar-icon" @click="isLayoutMenuOpen = !isLayoutMenuOpen"></i>
                  <!-- Layout Float Menu Mock -->
                  <div v-if="isLayoutMenuOpen" style="position: absolute; top: 100%; left: 0; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); display: flex; gap: 8px; padding: 8px; z-index: 100;">
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#DFE1E6"></div><div style="flex:1;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#DFE1E6"></div><div style="flex:2;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:2;background:#DFE1E6"></div><div style="flex:1;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #E6FCFF; border: 1px solid #0052CC; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#0052CC"></div><div style="flex:1;background:#0052CC"></div><div style="flex:1;background:#0052CC"></div></div>
                     <div style="width: 1px; height: 24px; background: #DFE1E6; margin: 0 4px;"></div>
                     <i class="fa-solid fa-trash" style="color: #5E6C84; line-height: 24px; cursor: pointer;"></i>
                  </div>
                </div>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-plus toolbar-icon"></i>
                <i class="fa-solid fa-chevron-down toolbar-icon" style="font-size: 10px; margin-left: -4px;"></i>
             </div>
             
             <!-- Editor Area -->
             <div style="padding: 24px; min-height: 200px; color: #172B4D; font-size: 14px; line-height: 1.5; outline: none;" contenteditable="true">
               <p style="margin: 0 0 16px 0; color: #5E6C84;">Dùng không gian này để chia sẻ bài học rút ra với 1 người theo dõi</p>
               <p v-if="isLayoutMenuOpen" style="margin: 0 0 16px 0;">Chúng tôi đã làm dự án chậm nhưng rất kỳ lạ thành công</p>
               <table v-if="isLayoutMenuOpen" style="width: 100%; border-collapse: collapse; margin-bottom: 16px;">
                 <tr>
                   <td style="border: 1px solid #DFE1E6; padding: 8px; background: #FAFBFC; height: 32px;"></td>
                   <td style="border: 1px solid #DFE1E6; padding: 8px; background: #FAFBFC;"></td>
                   <td style="border: 1px solid #DFE1E6; padding: 8px; background: #FAFBFC;"></td>
                 </tr>
                 <tr>
                   <td style="border: 1px solid #DFE1E6; padding: 8px; height: 32px;"></td>
                   <td style="border: 1px solid #DFE1E6; padding: 8px;"></td>
                   <td style="border: 1px solid #DFE1E6; padding: 8px;"></td>
                 </tr>
                 <tr>
                   <td style="border: 1px solid #DFE1E6; padding: 8px; height: 32px;"></td>
                   <td style="border: 1px solid #DFE1E6; padding: 8px;"></td>
                   <td style="border: 1px solid #DFE1E6; padding: 8px;"></td>
                 </tr>
               </table>
               <div v-if="isLayoutMenuOpen" style="display: flex; gap: 16px; margin-bottom: 16px;">
                  <div style="flex: 1; border: 1px dashed #DFE1E6; border-radius: 3px; height: 40px; background: #FAFBFC;"></div>
                  <div style="flex: 1; border: 1px dashed #DFE1E6; border-radius: 3px; height: 40px; background: #FAFBFC;"></div>
                  <div style="flex: 1; border: 1px dashed #DFE1E6; border-radius: 3px; height: 40px; background: #FAFBFC;"></div>
               </div>
             </div>
             
             <!-- Footer Actions -->
             <div style="padding: 12px 24px; border-top: 1px solid #DFE1E6; display: flex; gap: 8px; background: #FAFBFC;">
               <button class="primary-btn" @click="isEditingLearning = false">Save</button>
               <button class="cancel-btn" @click="isEditingLearning = false">Cancel</button>
             </div>
          </div>
        </template>

        <!-- RỦI RO TAB -->
        <template v-if="currentTab === 'risks'">
          <div v-if="!isEditingRisk" style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 40px; background: white; margin-top: 16px; display: flex; align-items: center; justify-content: center; gap: 32px;">
             <div style="position: relative; width: 64px; height: 64px; display: flex; justify-content: center; align-items: center;">
               <div style="width: 40px; height: 50px; background: #FFAB00; border-radius: 2px; position: relative; z-index: 1;">
                 <div style="position: absolute; top: 10px; left: 8px; right: 8px; height: 2px; background: #172B4D;"></div>
                 <div style="position: absolute; top: 18px; left: 8px; right: 8px; height: 2px; background: #172B4D;"></div>
                 <div style="position: absolute; top: 26px; left: 8px; right: 8px; height: 2px; background: #172B4D;"></div>
                 <div style="position: absolute; top: 34px; left: 8px; right: 8px; height: 2px; background: #172B4D;"></div>
               </div>
               <div style="position: absolute; right: 0; bottom: 5px; width: 8px; height: 30px; background: #FFC400; border-radius: 2px; transform: rotate(15deg); z-index: 2;">
                 <div style="position: absolute; bottom: -4px; left: 0; right: 0; height: 4px; background: #172B4D; border-bottom-left-radius: 4px; border-bottom-right-radius: 4px;"></div>
               </div>
             </div>
             <div style="max-width: 400px;">
                <h3 style="margin: 0 0 8px 0; font-size: 14px; color: #172B4D;">Nắm bắt các rủi ro đã biết</h3>
                <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Theo dõi mọi rủi ro liên quan đến mục tiêu này để tránh những bất ngờ sau này.</p>
                <div style="display: flex; gap: 16px; align-items: center;">
                  <button class="secondary-btn" @click="isEditingRisk = true" style="background: white; border: 1px solid #DFE1E6; font-weight: 600;">Thêm risk mới</button>
                </div>
             </div>
          </div>

          <div v-else style="border: 1px solid #4C9AFF; border-radius: 3px; background: white; margin-top: 16px; overflow: hidden; box-shadow: 0 0 0 1px #4C9AFF;">
             <div style="padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; align-items: center; gap: 12px;">
                <i class="fa-solid fa-triangle-exclamation" style="color: #FF5630; font-size: 18px;"></i>
                <input type="text" placeholder="Tóm tắt cho rủi ro của bạn là gì?" style="border: none; outline: none; font-size: 16px; color: #172B4D; width: 100%;" />
             </div>
             
             <!-- Rich Text Editor Toolbar Mockup -->
             <div style="display: flex; gap: 8px; padding: 8px 16px; border-bottom: 1px solid #DFE1E6; background: white; flex-wrap: wrap; align-items: center;">
                <div style="display: flex; align-items: center; gap: 4px; padding: 4px 8px; cursor: pointer; color: #5E6C84; border-radius: 3px;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                  <span style="font-size: 13px;">Normal text</span> <i class="fa-solid fa-chevron-down" style="font-size: 10px;"></i>
                </div>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-bold toolbar-icon"></i>
                <i class="fa-solid fa-italic toolbar-icon"></i>
                <i class="fa-solid fa-ellipsis toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <div style="display: flex; flex-direction: column; align-items: center; cursor: pointer; padding: 4px; border-radius: 3px;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                   <i class="fa-solid fa-font" style="font-size: 14px; color: #5E6C84;"></i>
                   <div style="width: 12px; height: 3px; background: #FF5630; margin-top: 2px;"></div>
                </div>
                <i class="fa-solid fa-chevron-down toolbar-icon" style="font-size: 10px; margin-left: -4px;"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-list-ul toolbar-icon"></i>
                <i class="fa-solid fa-list-ol toolbar-icon"></i>
                <i class="fa-regular fa-square-check toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-link toolbar-icon"></i>
                <i class="fa-solid fa-at toolbar-icon"></i>
                <i class="fa-regular fa-face-smile toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-table-cells toolbar-icon"></i>
                <div style="position: relative;">
                  <i class="fa-solid fa-columns toolbar-icon" @click="isLayoutMenuOpenRisk = !isLayoutMenuOpenRisk"></i>
                  <div v-if="isLayoutMenuOpenRisk" style="position: absolute; top: 100%; left: 0; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); display: flex; gap: 8px; padding: 8px; z-index: 100;">
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#DFE1E6"></div><div style="flex:1;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#DFE1E6"></div><div style="flex:2;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:2;background:#DFE1E6"></div><div style="flex:1;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #E6FCFF; border: 1px solid #0052CC; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#0052CC"></div><div style="flex:1;background:#0052CC"></div><div style="flex:1;background:#0052CC"></div></div>
                     <div style="width: 1px; height: 24px; background: #DFE1E6; margin: 0 4px;"></div>
                     <i class="fa-solid fa-trash" style="color: #5E6C84; line-height: 24px; cursor: pointer;"></i>
                  </div>
                </div>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-plus toolbar-icon"></i>
                <i class="fa-solid fa-chevron-down toolbar-icon" style="font-size: 10px; margin-left: -4px;"></i>
             </div>
             
             <!-- Editor Area -->
             <div style="padding: 24px; min-height: 200px; color: #172B4D; font-size: 14px; line-height: 1.5; outline: none;" contenteditable="true">
               <p style="margin: 0 0 16px 0; color: #5E6C84;">Sử dụng không gian này để chia sẻ rủi ro mới của bạn với 1 người theo dõi của bạn</p>
             </div>
             
             <!-- Footer Actions -->
             <div style="padding: 12px 24px; border-top: 1px solid #DFE1E6; display: flex; gap: 8px; background: #FAFBFC;">
               <button class="primary-btn" @click="isEditingRisk = false">Save</button>
               <button class="cancel-btn" @click="isEditingRisk = false">Cancel</button>
             </div>
          </div>
        </template>

        <!-- QUYẾT ĐỊNH TAB -->
        <template v-if="currentTab === 'decisions'">
          <div v-if="!isEditingDecision" style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 40px; background: white; margin-top: 16px; display: flex; align-items: center; justify-content: center; gap: 32px;">
             <div style="position: relative; width: 64px; height: 64px; display: flex; justify-content: center; align-items: center;">
               <i class="fa-solid fa-code-branch fa-rotate-270" style="font-size: 40px; color: #36B37E;"></i>
               <div style="position: absolute; top: 12px; left: 16px; width: 12px; height: 12px; background: #0052CC; transform: rotate(45deg);"></div>
               <div style="position: absolute; bottom: 12px; left: 16px; width: 12px; height: 12px; background: #6554C0; transform: rotate(45deg);"></div>
             </div>
             <div style="max-width: 400px;">
                <h3 style="margin: 0 0 8px 0; font-size: 14px; color: #172B4D;">Truyền đạt các quyết định lớn</h3>
                <p style="margin: 0 0 16px 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Ghi lại các quyết định lớn cho mục tiêu này tại đây để chia sẻ trong bản cập nhật mới nhất của bạn.</p>
                <div style="display: flex; gap: 16px; align-items: center;">
                  <button class="secondary-btn" @click="isEditingDecision = true" style="background: white; border: 1px solid #DFE1E6; font-weight: 600;">Thêm decision mới</button>
                </div>
             </div>
          </div>

          <div v-else style="border: 1px solid #4C9AFF; border-radius: 3px; background: white; margin-top: 16px; overflow: hidden; box-shadow: 0 0 0 1px #4C9AFF;">
             <div style="padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; align-items: center; gap: 12px;">
                <i class="fa-solid fa-code-branch fa-rotate-270" style="color: #36B37E; font-size: 18px;"></i>
                <input type="text" placeholder="Tóm tắt cho quyết định của bạn là gì?" style="border: none; outline: none; font-size: 16px; color: #172B4D; width: 100%;" />
             </div>
             
             <!-- Rich Text Editor Toolbar Mockup -->
             <div style="display: flex; gap: 8px; padding: 8px 16px; border-bottom: 1px solid #DFE1E6; background: white; flex-wrap: wrap; align-items: center;">
                <div style="display: flex; align-items: center; gap: 4px; padding: 4px 8px; cursor: pointer; color: #5E6C84; border-radius: 3px;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                  <span style="font-size: 13px;">Normal text</span> <i class="fa-solid fa-chevron-down" style="font-size: 10px;"></i>
                </div>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-bold toolbar-icon"></i>
                <i class="fa-solid fa-italic toolbar-icon"></i>
                <i class="fa-solid fa-ellipsis toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <div style="display: flex; flex-direction: column; align-items: center; cursor: pointer; padding: 4px; border-radius: 3px;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                   <i class="fa-solid fa-font" style="font-size: 14px; color: #5E6C84;"></i>
                   <div style="width: 12px; height: 3px; background: #FF5630; margin-top: 2px;"></div>
                </div>
                <i class="fa-solid fa-chevron-down toolbar-icon" style="font-size: 10px; margin-left: -4px;"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-list-ul toolbar-icon"></i>
                <i class="fa-solid fa-list-ol toolbar-icon"></i>
                <i class="fa-regular fa-square-check toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-link toolbar-icon"></i>
                <i class="fa-solid fa-at toolbar-icon"></i>
                <i class="fa-regular fa-face-smile toolbar-icon"></i>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-table-cells toolbar-icon"></i>
                <div style="position: relative;">
                  <i class="fa-solid fa-columns toolbar-icon" @click="isLayoutMenuOpenDecision = !isLayoutMenuOpenDecision"></i>
                  <div v-if="isLayoutMenuOpenDecision" style="position: absolute; top: 100%; left: 0; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); display: flex; gap: 8px; padding: 8px; z-index: 100;">
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#DFE1E6"></div><div style="flex:1;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#DFE1E6"></div><div style="flex:2;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #FAFBFC; border: 1px solid #DFE1E6; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:2;background:#DFE1E6"></div><div style="flex:1;background:#DFE1E6"></div></div>
                     <div style="width: 24px; height: 24px; background: #E6FCFF; border: 1px solid #0052CC; display: flex; gap: 2px; padding: 2px; cursor: pointer;"><div style="flex:1;background:#0052CC"></div><div style="flex:1;background:#0052CC"></div><div style="flex:1;background:#0052CC"></div></div>
                     <div style="width: 1px; height: 24px; background: #DFE1E6; margin: 0 4px;"></div>
                     <i class="fa-solid fa-trash" style="color: #5E6C84; line-height: 24px; cursor: pointer;"></i>
                  </div>
                </div>
                <div style="width: 1px; height: 16px; background: #DFE1E6;"></div>
                <i class="fa-solid fa-plus toolbar-icon"></i>
                <i class="fa-solid fa-chevron-down toolbar-icon" style="font-size: 10px; margin-left: -4px;"></i>
             </div>
             
             <!-- Editor Area -->
             <div style="padding: 24px; min-height: 200px; color: #172B4D; font-size: 14px; line-height: 1.5; outline: none;" contenteditable="true">
               <p style="margin: 0 0 16px 0; color: #5E6C84;">Sử dụng không gian này để chia sẻ quyết định của bạn với 1 người theo dõi</p>
             </div>
             
             <!-- Footer Actions -->
             <div style="padding: 12px 24px; border-top: 1px solid #DFE1E6; display: flex; gap: 8px; background: #FAFBFC;">
               <button class="primary-btn" @click="isEditingDecision = false">Save</button>
               <button class="cancel-btn" @click="isEditingDecision = false">Cancel</button>
             </div>
          </div>
        </template>
      </div>

      <!-- Right Column: Sidebar Details -->
      <div class="side-column">
        <div class="details-card">
          <div class="details-header">
            <h3>Chi tiết</h3>
          </div>
          <div class="details-body">
            <!-- Tiến độ -->
            <div class="detail-row">
              <div class="detail-label">Tiến độ</div>
              <div class="detail-value progress-value">
                <div class="progress-bar-bg"><div class="progress-bar-fill" :style="{ width: (goal.progress || 0) + '%' }"></div></div>
                <span>{{ goal.progress || 0 }}%</span>
              </div>
            </div>

            <!-- Key results -->
            <div class="detail-row">
              <div class="detail-label">Key results <span class="badge-count">0</span></div>
              <div class="detail-value progress-value">
                <div class="progress-bar-bg"><div class="progress-bar-fill" style="width: 0%"></div></div>
                <span>0%</span>
              </div>
            </div>

            <!-- Chủ sở hữu -->
            <div class="detail-row">
              <div class="detail-label">Chủ sở hữu</div>
              <div class="detail-value">
                <div class="owner-chip">
                  <div class="owner-avatar-micro">{{ goal.creatorName ? goal.creatorName.substring(0,1) : (goal.owner ? goal.owner.substring(0,1) : 'U') }}</div>
                  <span>{{ goal.creatorName || goal.owner || 'Chưa có' }}</span>
                </div>
              </div>
            </div>

            <!-- Người theo dõi -->
            <div class="detail-row">
              <div class="detail-label">Người theo dõi <button class="icon-btn-micro" @click="isShareModalOpen = true"><i class="fa-solid fa-plus"></i></button></div>
              <div class="detail-value flex-between">
                <span class="empty-value" style="cursor: pointer;" @click="isShareModalOpen = true">Thêm người theo dõi</span>
                <div class="follower-icons">
                  <i class="fa-brands fa-slack ms-1"></i>
                  <i class="fa-brands fa-microsoft ms-1"></i>
                </div>
              </div>
            </div>

            <!-- Mục tiêu chính -->
            <div class="detail-row relative-popover-container">
              <div class="detail-label">Mục tiêu chính <button class="icon-btn-micro" v-if="!linkedParentGoal" @click.stop="togglePopover('parentGoal')"><i class="fa-solid fa-plus"></i></button></div>
              
              <div class="detail-value" v-if="linkedParentGoal">
                <div class="linked-item">
                  <i class="fa-solid fa-bullseye item-icon"></i>
                  <span class="item-name">{{ linkedParentGoal.name }}</span>
                  <button class="remove-btn" @click="linkedParentGoal = null"><i class="fa-solid fa-xmark"></i></button>
                </div>
              </div>

              <!-- Parent Goal Popover -->
              <div class="custom-popover" v-if="popovers.parentGoal" @click.stop>
                <input type="text" class="popover-search" placeholder="Tìm kiếm mục tiêu hoặc dán liên kết" v-model="searchQueries.parentGoal" />
                <div class="popover-list-title">Kết quả</div>
                <div class="popover-list">
                  <div class="popover-item" v-for="g in mockGoalsList" :key="g.id" @click="setParentGoal(g)">
                    <i class="fa-solid fa-bullseye item-icon-muted"></i>
                    <div class="item-details">
                      <div class="item-name">{{ g.name }}</div>
                      <div class="item-meta">{{ g.owner }}</div>
                    </div>
                  </div>
                </div>
                <div style="padding: 12px 12px 0; border-top: 1px solid #DFE1E6; margin-top: 8px;">
                  <span style="font-size: 14px; color: #172B4D; cursor: pointer; display: flex; align-items: center; gap: 8px;" onmouseover="this.style.textDecoration='underline'" onmouseout="this.style.textDecoration='none'"><i class="fa-solid fa-plus"></i> Tạo mục tiêu</span>
                </div>
              </div>
            </div>

            <!-- Mục tiêu phụ -->
            <div class="detail-row relative-popover-container">
              <div class="detail-label">Mục tiêu phụ <button class="icon-btn-micro" @click.stop="togglePopover('subGoals')"><i class="fa-solid fa-plus"></i></button></div>
              
              <div class="detail-value" v-if="linkedSubGoals.length > 0">
                <div class="linked-item" v-for="g in linkedSubGoals" :key="g.id">
                  <i class="fa-solid fa-bullseye item-icon"></i>
                  <span class="item-name">{{ g.name }}</span>
                  <button class="remove-btn" @click="removeSubGoal(g.id)"><i class="fa-solid fa-xmark"></i></button>
                </div>
              </div>

              <!-- Sub Goals Popover -->
              <div class="custom-popover" v-if="popovers.subGoals" @click.stop>
                <input type="text" class="popover-search" placeholder="Tìm kiếm mục tiêu hoặc dán liên kết" v-model="searchQueries.subGoals" />
                <div class="popover-list-title">Kết quả</div>
                <div class="popover-list">
                  <div class="popover-item" v-for="g in mockGoalsList" :key="g.id" @click="addSubGoal(g)">
                    <i class="fa-solid fa-bullseye item-icon-muted"></i>
                    <div class="item-details">
                      <div class="item-name">{{ g.name }}</div>
                      <div class="item-meta">{{ g.owner }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Loại -->
            <div class="detail-row">
              <div class="detail-label">Loại</div>
              <div class="detail-value" style="display: flex; align-items: center; gap: 8px; font-size: 14px; color: #172B4D;">
                <i class="fa-solid fa-bullseye"></i> Mục tiêu
              </div>
            </div>

            <!-- Nhóm -->
            <div class="detail-row relative-popover-container">
              <div class="detail-label">Nhóm <button class="icon-btn-micro" @click.stop="togglePopover('teams')"><i class="fa-solid fa-plus"></i></button></div>
              
              <div class="detail-value" v-if="linkedTeams.length > 0">
                <div class="linked-item team-item-chip" v-for="t in linkedTeams" :key="t.id">
                  <div class="team-icon" :style="{ backgroundColor: t.color }"><i class="fa-solid fa-users"></i></div>
                  <span class="item-name">{{ t.name }} <i class="fa-solid fa-circle-check text-blue" style="font-size: 12px;" v-if="t.verified"></i></span>
                  <button class="remove-btn" @click="removeTeam(t.id)"><i class="fa-solid fa-xmark"></i></button>
                </div>
              </div>

              <!-- Teams Popover -->
              <div class="custom-popover popover-large" v-if="popovers.teams" @click.stop style="width: 320px;">
                <div class="search-input-with-icon">
                  <i class="fa-solid fa-user-group icon-left"></i>
                  <input type="text" class="popover-search with-icon" placeholder="Tìm kiếm đội ngũ" v-model="searchQueries.teams" />
                </div>
                <div class="popover-list mt-2" style="max-height: 250px; overflow-y: auto;">
                  <div class="popover-item team-select-item" v-for="t in mockTeamsList" :key="t.id" @click="addTeam(t)">
                    <div class="team-icon-large" :style="{ backgroundColor: t.color }"><i class="fa-solid fa-users"></i></div>
                    <div class="item-details">
                      <div class="item-name" style="font-weight: 500;">{{ t.name }} <i class="fa-solid fa-circle-check text-blue" style="font-size: 12px;" v-if="t.verified"></i></div>
                      <div class="item-meta">Đội ngũ chính thức • {{ t.members }} thành viên, kể cả bạn</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Ngày bắt đầu -->
            <div class="detail-row relative-popover-container">
              <div class="detail-label">Ngày bắt đầu <button class="icon-btn-micro" @click.stop="togglePopover('startDate')"><i class="fa-solid fa-plus"></i></button></div>
              <div class="detail-value" v-if="startDate"><span class="item-name">{{ formattedStartDate }}</span></div>

              <!-- Start Date Popover -->
              <div class="custom-popover" v-if="popovers.startDate" @click.stop style="width: 300px; padding: 0;">
                <div style="padding: 16px; border-bottom: 1px solid #DFE1E6;">
                  <h4 style="margin: 0 0 8px 0; font-size: 14px; font-weight: 600; color: #172B4D;">Ngày bắt đầu</h4>
                  <div class="date-input-mock">
                    <input type="text" placeholder="Nhập ngày bắt đầu" v-model="formattedStartDate" readonly />
                    <i class="fa-regular fa-calendar"></i>
                  </div>
                </div>
                <div style="padding: 16px;">
                  <div class="calendar-header">
                    <button class="cal-nav-btn"><i class="fa-solid fa-angles-left"></i></button>
                    <button class="cal-nav-btn"><i class="fa-solid fa-angle-left"></i></button>
                    <span class="cal-month-year">June 2026</span>
                    <button class="cal-nav-btn"><i class="fa-solid fa-angle-right"></i></button>
                    <button class="cal-nav-btn"><i class="fa-solid fa-angles-right"></i></button>
                  </div>
                  <div class="calendar-grid">
                    <div class="cal-day-header">Sun</div><div class="cal-day-header">Mon</div><div class="cal-day-header">Tue</div><div class="cal-day-header">Wed</div><div class="cal-day-header">Thu</div><div class="cal-day-header">Fri</div><div class="cal-day-header">Sat</div>
                    <div class="cal-day muted">31</div><div class="cal-day">1</div><div class="cal-day">2</div><div class="cal-day">3</div><div class="cal-day">4</div><div class="cal-day">5</div><div class="cal-day">6</div>
                    <div class="cal-day">7</div><div class="cal-day">8</div><div class="cal-day">9</div><div class="cal-day">10</div><div class="cal-day">11</div><div class="cal-day">12</div><div class="cal-day">13</div>
                    <div class="cal-day">14</div><div class="cal-day">15</div><div class="cal-day active">16</div><div class="cal-day">17</div><div class="cal-day">18</div><div class="cal-day">19</div><div class="cal-day">20</div>
                    <div class="cal-day">21</div><div class="cal-day">22</div><div class="cal-day">23</div><div class="cal-day">24</div><div class="cal-day">25</div><div class="cal-day">26</div><div class="cal-day">27</div>
                    <div class="cal-day">28</div><div class="cal-day">29</div><div class="cal-day">30</div><div class="cal-day muted">1</div><div class="cal-day muted">2</div><div class="cal-day muted">3</div><div class="cal-day muted">4</div>
                  </div>
                  <div style="font-size: 11px; color: #0052CC; margin-top: 12px;">Đã tạo mục tiêu này vào 7 Jun 2026</div>
                </div>
                <div class="popover-actions" style="padding: 12px 16px; border-top: 1px solid #DFE1E6; display: flex; justify-content: space-between;">
                  <button class="secondary-btn" style="flex: 1;" @click="popovers.startDate = false">Hủy</button>
                  <button class="primary-btn" style="flex: 1;" @click="saveStartDate">Lưu</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Share Modal -->
    <ShareModal 
      :isOpen="isShareModalOpen" 
      :projectId="route.params.id" 
      :projectName="goal?.name" 
      @close="isShareModalOpen = false" 
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useGoalStore } from '@/store/useGoalStore'
import RichTextEditor from '@/components/common/RichTextEditor.vue'
import ShareModal from '@/components/common/ShareModal.vue'

const route = useRoute()
const router = useRouter()
const goalStore = useGoalStore()

const showUpdateForm = ref(false)
const isUpdateStatusOpen = ref(false)
const newUpdate = ref({ status: 'ĐÚNG TIẾN ĐỘ', message: '', progress: 0 })

const isJiraInputOpen = ref(false)
const isProjectSearchOpen = ref(false)
const isEditingLearning = ref(false)
const isEditingRisk = ref(false)
const isEditingDecision = ref(false)
const isLayoutMenuOpen = ref(false)
const isLayoutMenuOpenRisk = ref(false)
const isLayoutMenuOpenDecision = ref(false)

const currentTab = ref('overview')
const activityTab = ref('comments')

const isEditingBio = ref(false)
const tempBio = ref('')

const goal = computed(() => goalStore.currentGoal)
const updates = computed(() => goalStore.updates || [])

// Mocks for subgoals
const subGoals = ref([])

const isShareModalOpen = ref(false)

const popovers = ref({
  parentGoal: false,
  subGoals: false,
  teams: false,
  startDate: false
})

const togglePopover = (type) => {
  for (const key in popovers.value) {
    if (key === type) popovers.value[key] = !popovers.value[key]
    else popovers.value[key] = false
  }
}

const closePopovers = (e) => {
  if (!e.target.closest('.custom-popover') && !e.target.closest('.icon-btn-micro')) {
    popovers.value = { parentGoal: false, subGoals: false, teams: false, startDate: false }
  }
}

const searchQueries = ref({ parentGoal: '', subGoals: '', teams: '' })

const linkedParentGoal = ref(null)
const linkedSubGoals = ref([])
const linkedTeams = ref([])
const startDate = ref(null)
const formattedStartDate = ref('16 Jun 2026')

const mockGoalsList = [
  { id: 'g1', name: 'iPhone 15 Pro Max', owner: 'Tua20000' },
  { id: 'g2', name: 'uew', owner: 'Tua20000' }
]

const mockTeamsList = [
  { id: 't1', name: '###', members: 1, color: '#36B37E', verified: true },
  { id: 't2', name: '30', members: 1, color: '#FF5630', verified: true },
  { id: 't3', name: 'RRRR', members: 1, color: '#FF5630', verified: true }
]

const setParentGoal = (g) => {
  linkedParentGoal.value = g
  popovers.value.parentGoal = false
}

const addSubGoal = (g) => {
  if (!linkedSubGoals.value.find(x => x.id === g.id)) linkedSubGoals.value.push(g)
  popovers.value.subGoals = false
}
const removeSubGoal = (id) => { linkedSubGoals.value = linkedSubGoals.value.filter(x => x.id !== id) }

const addTeam = (t) => {
  if (!linkedTeams.value.find(x => x.id === t.id)) linkedTeams.value.push(t)
  popovers.value.teams = false
}
const removeTeam = (id) => { linkedTeams.value = linkedTeams.value.filter(x => x.id !== id) }

const saveStartDate = () => {
  startDate.value = '2026-06-16'
  popovers.value.startDate = false
}

const startEditingBio = () => {
  tempBio.value = goal.value?.description || ''
  isEditingBio.value = true
}

const saveBio = async (val) => {
  if (goal.value) {
    // API call to update bio
    goal.value.description = typeof val === 'string' ? val : tempBio.value
  }
  isEditingBio.value = false
}

onMounted(async () => {
  window.addEventListener('click', closePopovers)
  if (route.params.id) {
    await goalStore.fetchGoalDetail(route.params.id)
  }
})

const getStatusClass = (status) => {
  if (!status) return 'status-pending'
  const map = {
    'đúng tiến độ': 'status-on-track',
    'có rủi ro': 'status-at-risk',
    'trễ tiến độ': 'status-off-track',
    'đang chờ cập nhật': 'status-pending',
    'đã hoàn tất': 'status-done',
    'đã lưu trữ': 'status-archived'
  }
  return map[status.toLowerCase()] || 'status-pending'
}

const toggleFollow = () => {
  if (goal.value) goalStore.toggleFollow(goal.value.id)
}

const toggleStar = () => {
  goalStore.toggleStar()
}

const postUpdate = () => {
  showUpdateForm.value = false
  newUpdate.value.message = ''
  // API Call would go here
}
</script>

<style scoped>
.goal-detail-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #FAFBFC;
}

/* Module Header Styles */
.module-header {
  padding: 32px 40px 0;
  background-color: #FFFFFF;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header-content h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.primary-btn:hover {
  background-color: #0047B3;
}

.tabs-nav {
  display: flex;
  border-bottom: 2px solid #DFE1E6;
  gap: 24px;
}

.tab-link {
  padding: 8px 0 12px;
  font-size: 14px;
  font-weight: 500;
  color: #5E6C84;
  text-decoration: none;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
  transition: color 0.2s;
}

.tab-link:hover {
  color: #172B4D;
}

.tab-link.active {
  color: #0052CC;
  border-bottom-color: #0052CC;
}

/* Entity Header Styles */
.goal-header {
  padding: 32px 0 24px;
  background-color: #FFFFFF;
}

.goal-header-inner {
  max-width: 1000px;
  margin: 0 auto;
  padding: 0 40px;
  width: 100%;
}

.header-breadcrumbs {
  font-size: 14px;
  color: #5E6C84;
  margin-bottom: 16px;
}

.header-breadcrumbs a {
  color: #5E6C84;
  text-decoration: none;
}

.header-breadcrumbs a:hover {
  text-decoration: underline;
  color: #0052CC;
}

.separator {
  margin: 0 8px;
}

.current-crumb {
  color: #172B4D;
}

.header-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.title-block {
  display: flex;
  align-items: center;
  gap: 12px;
}

.goal-icon-large {
  width: 32px;
  height: 32px;
  background-color: #0052CC;
  color: white;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
}

.title-block h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.secondary-btn {
  background-color: rgba(9, 30, 66, 0.04);
  color: #42526E;
  border: none;
  padding: 6px 12px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: background-color 0.2s;
}

.secondary-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.secondary-btn.icon-only {
  padding: 6px 8px;
}

.secondary-btn.starred i {
  color: #FFAB00;
}

.quick-status-row {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-left: 44px; /* Align with title text */
}

/* Status Badge */
.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 2px 8px;
  border-radius: 3px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
}

.status-badge-small {
  display: inline-flex;
  align-items: center;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

/* Status Colors */
.status-on-track { background-color: #E3FCEF; color: #006644; }
.status-on-track .status-dot { background-color: #36B37E; }

.status-at-risk { background-color: #FFF0B3; color: #FF8B00; }
.status-at-risk .status-dot { background-color: #FFAB00; }

.status-off-track { background-color: #FFEBE6; color: #BF2600; }
.status-off-track .status-dot { background-color: #FF5630; }

.status-done { background-color: #EAE6FF; color: #403294; }
.status-done .status-dot { background-color: #6554C0; }

.status-pending { background-color: #DFE1E6; color: #42526E; }
.status-pending .status-dot { background-color: #7A869A; }

.update-text {
  font-size: 13px;
  color: #5E6C84;
}

/* Content Grid */
.goal-content-grid {
  display: grid;
  grid-template-columns: minmax(0, 2fr) 300px;
  gap: 40px;
  padding: 32px 40px;
  max-width: 1000px;
  margin: 0 auto;
  width: 100%;
}

.main-column {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.content-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #DFE1E6;
  padding-bottom: 8px;
}

.section-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #172B4D;
}

.icon-btn-small {
  background: none;
  border: none;
  color: #5E6C84;
  cursor: pointer;
  padding: 4px;
  border-radius: 3px;
}

.icon-btn-small:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.empty-text {
  color: #5E6C84;
  font-style: italic;
  font-size: 14px;
}

/* Empty States */
.empty-state-micro {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 32px;
  background-color: #FAFBFC;
  border: 1px dashed #DFE1E6;
  border-radius: 3px;
  color: #5E6C84;
  text-align: center;
}

.empty-icon-micro {
  font-size: 24px;
  margin-bottom: 8px;
  color: #A5ADBA;
}

/* Status Updates Area */
.status-update-box {
  margin-bottom: 24px;
}

.update-input-mockup {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
}

.user-avatar-current {
  width: 32px;
  height: 32px;
  background-color: #0052CC;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
}

.fake-input {
  flex: 1;
  padding: 10px 16px;
  border: 1px solid #DFE1E6;
  border-radius: 24px;
  color: #5E6C84;
  font-size: 14px;
  transition: border-color 0.2s;
}

.fake-input:hover {
  border-color: #A5ADBA;
}

.update-form-active {
  background-color: #FFFFFF;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 16px;
  box-shadow: 0 4px 8px -2px rgba(9,30,66,0.25);
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
  margin-bottom: 8px;
}

.jira-select, .jira-textarea {
  width: 100%;
  padding: 8px 12px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  font-family: inherit;
  outline: none;
  box-sizing: border-box;
}

.jira-select:focus, .jira-textarea:focus {
  border-color: #4C9AFF;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
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
}

.primary-btn:hover:not(:disabled) {
  background-color: #0047B3;
}

.primary-btn:disabled {
  background-color: #EBECF0;
  color: #A5ADBA;
  cursor: not-allowed;
}

.cancel-btn {
  background: transparent;
  color: #5E6C84;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
}

.cancel-btn:hover {
  background: rgba(9,30,66,0.08);
}

/* Timeline */
.timeline-list {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.timeline-item {
  display: flex;
  gap: 16px;
}

.timeline-avatar {
  width: 32px;
  height: 32px;
  background-color: #172B4D;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
}

.timeline-content {
  flex: 1;
}

.timeline-meta {
  font-size: 14px;
  color: #172B4D;
  margin-bottom: 4px;
}

.timeline-date {
  color: #5E6C84;
  margin-left: 8px;
  font-size: 12px;
}

.timeline-status-change {
  margin-bottom: 8px;
}

.timeline-message {
  font-size: 14px;
  color: #172B4D;
  line-height: 1.5;
  background-color: #FAFBFC;
  padding: 12px;
  border-radius: 3px;
  border: 1px solid #DFE1E6;
}

.empty-timeline {
  color: #5E6C84;
  font-size: 14px;
  font-style: italic;
}

/* Sidebar Details */
.details-card {
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  background-color: #FFFFFF;
}

.details-header {
  padding: 16px;
  border-bottom: 1px solid #DFE1E6;
}

.details-header h3 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: #172B4D;
  text-transform: uppercase;
}

.details-body {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.detail-row {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-label {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
}

.icon-btn-micro {
  background: transparent;
  border: none;
  color: #5E6C84;
  cursor: pointer;
  width: 24px;
  height: 24px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s, color 0.2s;
}

.icon-btn-micro:hover {
  background-color: rgba(9, 30, 66, 0.08);
  color: #172B4D;
}

.detail-value {
  font-size: 14px;
  color: #172B4D;
}

.empty-value {
  color: #5E6C84;
  font-style: italic;
  font-size: 13px;
}

/* Progress in sidebar */
.progress-value {
  display: flex;
  align-items: center;
  gap: 8px;
}

.progress-bar-bg {
  flex: 1;
  height: 6px;
  background-color: #DFE1E6;
  border-radius: 3px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  background-color: #0052CC;
  border-radius: 3px;
}

/* Owner chip */
.owner-chip {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.owner-avatar-micro {
  width: 20px;
  height: 20px;
  background-color: #0052CC;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.tab-btn {
  background: none;
  border: none;
  padding: 12px 0;
  font-size: 14px;
  font-weight: 500;
  color: #5E6C84;
  cursor: pointer;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
  transition: color 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}

.tab-btn:hover {
  color: #172B4D;
}

.tab-btn.active {
  color: #0052CC;
  border-bottom-color: #0052CC;
}

.badge-count {
  background: #EBECF0;
  color: #172B4D;
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 12px;
  font-weight: 600;
}

.toggle-btn {
  background: none;
  border: none;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  color: #5E6C84;
  cursor: pointer;
  border-radius: 3px;
}

.toggle-btn:hover {
  background: rgba(9,30,66,0.04);
}

.toggle-btn.active {
  background: rgba(9,30,66,0.08);
  color: #172B4D;
}

.toolbar-icon {
  color: #5E6C84;
  font-size: 14px;
  padding: 6px;
  border-radius: 3px;
  cursor: pointer;
  transition: background 0.2s;
  font-size: 12px;
}
.toolbar-icon:hover {
  background: #FAFBFC;
}

.reaction-btn:hover {
  background-color: #FAFBFC;
}

/* Sidebar Popovers (Shared with ProjectDetail) */
.relative-popover-container {
  position: relative;
}

.custom-popover {
  position: absolute;
  top: 100%;
  right: 0;
  width: 300px;
  background: white;
  border-radius: 3px;
  box-shadow: 0 4px 8px -2px rgba(9,30,66,0.25), 0 0 1px rgba(9,30,66,0.31);
  padding: 12px;
  z-index: 100;
  margin-top: 4px;
}

.custom-popover.popover-large {
  width: 350px;
  padding: 16px;
}

.popover-search {
  width: 100%;
  padding: 6px 8px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.popover-search:focus {
  border-color: #4C9AFF;
}

.search-input-with-icon {
  position: relative;
}

.search-input-with-icon .icon-left {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: #5E6C84;
}

.popover-search.with-icon {
  padding-left: 32px;
}

.popover-list-title {
  font-size: 11px;
  font-weight: 700;
  color: #6B778C;
  margin-top: 12px;
  margin-bottom: 8px;
}

.popover-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.popover-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px;
  border-radius: 3px;
  cursor: pointer;
}

.popover-item:hover {
  background-color: #F4F5F7;
}

.item-icon-muted {
  color: #5E6C84;
  font-size: 16px;
  width: 16px;
  text-align: center;
}

.item-details {
  display: flex;
  flex-direction: column;
}

.item-name {
  font-size: 14px;
  color: #172B4D;
}

.item-meta {
  font-size: 12px;
  color: #5E6C84;
}

/* Team Icons and Chips */
.team-icon {
  width: 20px;
  height: 20px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 10px;
}

.team-icon-large {
  width: 32px;
  height: 32px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 16px;
}

.team-item-chip {
  display: flex;
  align-items: center;
  gap: 8px;
}

.team-select-item {
  padding: 8px;
  border-radius: 3px;
}

.team-select-item:hover {
  background-color: #F4F5F7;
}

.text-blue {
  color: #0052CC;
}

/* Start Date Calendar Custom Styles */
.date-input-mock {
  position: relative;
}
.date-input-mock input {
  width: 100%;
  padding: 6px 8px 6px 32px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  color: #172B4D;
  box-sizing: border-box;
  background-color: #FAFBFC;
}
.date-input-mock i {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: #5E6C84;
}
.calendar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}
.cal-nav-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  color: #5E6C84;
  padding: 4px;
  border-radius: 3px;
}
.cal-nav-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}
.cal-month-year {
  font-weight: 600;
  font-size: 14px;
  color: #172B4D;
}
.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px;
  text-align: center;
}
.cal-day-header {
  font-size: 11px;
  font-weight: 600;
  color: #6B778C;
  padding-bottom: 8px;
}
.cal-day {
  font-size: 14px;
  color: #172B4D;
  padding: 6px 0;
  border-radius: 3px;
  cursor: pointer;
}
.cal-day:hover {
  background-color: #F4F5F7;
}
.cal-day.muted {
  color: #A5ADBA;
}
.cal-day.active {
  background-color: #0052CC;
  color: white;
  font-weight: 600;
}
.cal-day.active:hover {
  background-color: #0047B3;
}

/* Badge Count */
.badge-count {
  background-color: #DFE1E6;
  color: #172B4D;
  font-size: 12px;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 12px;
  margin-left: 8px;
}

.linked-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 8px;
  background-color: #F4F5F7;
  border-radius: 3px;
  margin-top: 8px;
  group-hover: block; /* Custom handling for remove btn */
}

.linked-item .remove-btn {
  margin-left: auto;
  background: transparent;
  border: none;
  color: #5E6C84;
  cursor: pointer;
  opacity: 0;
  transition: opacity 0.2s;
  padding: 4px;
}

.linked-item:hover .remove-btn {
  opacity: 1;
}

.linked-item .remove-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
  border-radius: 3px;
}
</style>
