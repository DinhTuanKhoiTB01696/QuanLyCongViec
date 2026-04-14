<template>
  <NexusLayout>


      <!-- Task Detail Modal Overlay -->
            <!-- Task Detail Modal Component -->
      <TaskDetailModal 
        v-if="showTaskModal" 
        :selectedTask="selectedTask" 
        :projectId="projectId"
        @close="showTaskModal = false"
        @updateTask="updateTaskField"
      />


      <!-- Nexus Layout handles Topbar and Sidebar -->
          <div class="page-header">
            <div class="header-breadcrumbs">Spaces</div>
            <div class="header-main-title">
              <div class="project-info-left">
                <div class="project-brand-icon">
                  <div class="inner-icon">
                    <div class="line header"></div>
                    <div class="line long"></div>
                    <div class="line mid"></div>
                  </div>
                </div>
                <h1 class="page-title">My Team</h1>
                <div class="project-info-right">
                  <div class="users-icon-box" title="Thành viên" @click="openMembersDialog">
                    <i class="fa-solid fa-users"></i>
                  </div>
                  <div class="more-icon-box" title="Thêm tùy chọn">
                  <el-dropdown trigger="click" placement="bottom-start" popper-class="space-settings-dropdown" @command="handleSpaceMenuCommand">
                    <i class="fa-solid fa-ellipsis"></i>
                    <template #dropdown>
                      <el-dropdown-menu>
                        <el-dropdown-item command="star"><i :class="isStarred ? 'fa-solid fa-star' : 'fa-regular fa-star'" :style="{ color: isStarred ? '#f59e0b' : '' }"></i> {{ isStarred ? 'Remove from starred' : 'Add to starred' }}</el-dropdown-item>
                        <el-dropdown-item command="add-people"><i class="fa-regular fa-user-plus"></i> Add people</el-dropdown-item>
                        <el-dropdown-item class="flex-between" command="save-template">
                          <span><i class="fa-regular fa-clone"></i> Save as template</span>
                          <span class="enterprise-badge">ENTERPRISE</span>
                        </el-dropdown-item>
                        <el-dropdown-item class="flex-between" command="set-background">
                          <span><i class="fa-solid fa-mountain-sun"></i> Set space background</span>
                          <i class="fa-solid fa-chevron-right sub-arrow"></i>
                        </el-dropdown-item>
                        <el-dropdown-item command="settings"><i class="fa-solid fa-gear"></i> Space settings</el-dropdown-item>
                        
                        <div class="dropdown-divider"></div>
                        
                        <el-dropdown-item command="archive"><i class="fa-solid fa-box-archive"></i> Archive space</el-dropdown-item>
                        <el-dropdown-item class="danger-item" command="delete"><i class="fa-solid fa-trash-can"></i> Delete space</el-dropdown-item>
                        
                        <div class="dropdown-divider"></div>

                        <el-dropdown-item class="info-item" disabled>
                          <div class="info-item-content">
                            <i class="fa-solid fa-rocket info-icon"></i>
                            <div class="info-text">
                              <div class="primary">Software space</div>
                              <div class="secondary">Team-managed</div>
                            </div>
                          </div>
                        </el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>
                </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Tabs -->
          <div class="jira-tabs">
            <div class="jira-tab" :class="{ active: currentTab === 'list' }" @click="currentTab = 'list'">Danh sách</div>
            <div class="jira-tab" :class="{ active: currentTab === 'board' }" @click="currentTab = 'board'">Bảng</div>
            <div class="jira-tab" :class="{ active: currentTab === 'summary' }" @click="currentTab = 'summary'">Tổng quan</div>
            <div class="jira-tab" :class="{ active: currentTab === 'backlog' }" @click="currentTab = 'backlog'">Tồn đọng</div>
            <div class="jira-tab" :class="{ active: currentTab === 'calendar' }" @click="currentTab = 'calendar'">Lịch</div>
            <div class="jira-tab" :class="{ active: currentTab === 'timeline' }" @click="currentTab = 'timeline'">Lộ trình</div>
            <div class="jira-tab" :class="{ active: currentTab === 'page' }" @click="currentTab = 'page'">Trang</div>
            <div class="jira-tab" :class="{ active: currentTab === 'forms' }" @click="currentTab = 'forms'">Biểu mẫu</div>
            <div class="tab-spacer"></div>
            <div class="jira-tab-icon"><i class="fa-solid fa-filter"></i> Bộ lọc</div>
            <div class="jira-tab-icon"><i class="fa-solid fa-sort"></i> Sắp xếp</div>
          </div>

          <!-- TAB CONTENT: SUMMARY (Grid Charts) -->
          <div class="summary-content" v-if="currentTab === 'summary'">
            <div class="filter-bar">
              <el-button size="small" plain class="dark-btn"><i class="fa-solid fa-filter"></i> Filter</el-button>
            </div>

            <div class="top-widgets">
              <div class="widget">
                <i class="fa-regular fa-circle-check widget-icon" style="color: #94a3b8"></i>
                <div class="widget-info">
                  <div class="num">{{ completedTasksLast7Days }} completed</div>
                  <div class="sub">in the last 7 days</div>
                </div>
              </div>
              <div class="widget">
                <i class="fa-solid fa-pen widget-icon" style="color: #94a3b8"></i>
                <div class="widget-info">
                  <div class="num">{{ updatedTasksLast7Days }} updated</div>
                  <div class="sub">in the last 7 days</div>
                </div>
              </div>
              <div class="widget">
                <i class="fa-regular fa-square-plus widget-icon" style="color: #94a3b8"></i>
                <div class="widget-info">
                  <div class="num">{{ createdTasksLast7Days }} created</div>
                  <div class="sub">in the last 7 days</div>
                </div>
              </div>
              <div class="widget">
                <i class="fa-regular fa-calendar widget-icon" style="color: #f59e0b"></i>
                <div class="widget-info">
                  <div class="num">{{ dueSoonTasksNext7Days }} due soon</div>
                  <div class="sub">in the next 7 days</div>
                </div>
              </div>
            </div>

            <!-- Dashboard Charts Grid -->
            <div class="charts-grid">
              <!-- Status overview -->
              <div class="chart-card">
                <div class="chart-header">
                  <h4>Tổng quan trạng thái</h4>
                  <p>Xem nhanh trạng thái các công việc của bạn.</p>
                </div>
                <div class="chart-body donut-body custom-legend-body">
                  <div id="status-donut-chart" style="width: 180px; height: 180px; flex-shrink: 0;"></div>
                  <div class="custom-status-legend">
                    <div class="legend-item" @mouseenter="handleStatusHover('DONE')" @mouseleave="handleStatusLeave()">
                       <div class="l-color" style="background: #86efac"></div> Đã hoàn thành : {{ statusCounts['DONE'] || 0 }}
                       <transition name="fade">
                         <div class="l-tooltip" v-if="hoveredStatus === 'DONE'">Đã hoàn thành: {{ statusCounts['DONE'] || 0 }} (Hoàn thành trong vòng 7 ngày qua)</div>
                       </transition>
                    </div>
                    <div class="legend-item" @mouseenter="handleStatusHover('TODO')" @mouseleave="handleStatusLeave()">
                       <div class="l-color" style="background: #c4b5fd"></div> Việc cần làm : {{ statusCounts['TODO'] || 0 }}
                       <transition name="fade">
                         <div class="l-tooltip" v-if="hoveredStatus === 'TODO'">Việc cần làm: {{ statusCounts['TODO'] || 0 }} (Tạo mới trong vòng 7 ngày qua)</div>
                       </transition>
                    </div>
                    <div class="legend-item" @mouseenter="handleStatusHover('INPROGRESS')" @mouseleave="handleStatusLeave()">
                       <div class="l-color" style="background: #93c5fd"></div> Đang thực hiện : {{ statusCounts['IN PROGRESS'] || 0 }}
                       <transition name="fade">
                         <div class="l-tooltip" v-if="hoveredStatus === 'INPROGRESS'">Đang thực hiện: {{ statusCounts['IN PROGRESS'] || 0 }}</div>
                       </transition>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Priority breakdown -->
              <div class="chart-card">
                <div class="chart-header">
                  <h4>Phân bổ độ ưu tiên</h4>
                  <p>Xem cái nhìn tổng thể về cách ưu tiên công việc.</p>
                </div>
                <div class="chart-body bar-chart">
                  <div id="priority-bar-chart" style="width: 100%; height: 250px;"></div>
                </div>
              </div>

              <!-- Types of work -->
              <div class="chart-card">
                <div class="chart-header">
                  <h4>Các loại công việc</h4>
                  <p>Xem chi tiết các hạng mục công việc theo loại. <a href="#" class="view-all-link">Xem tất cả các hạng mục</a></p>
                </div>
                <div class="chart-body type-progress-list">
                   <div class="type-row type-header-row">
                     <div class="t-col-name">Kiểu</div>
                     <div class="t-col-bar">Phân bổ</div>
                   </div>
                   <div class="type-row" v-for="(item, idx) in taskTypesBreakdown" :key="idx">
                     <div class="t-col-name">
                       <i :class="item.icon" :style="{ color: item.color }"></i>
                       <span>{{ item.label }}</span>
                     </div>
                     <div class="t-col-bar">
                       <div class="p-bar-bg">
                         <div class="p-bar-fill" :style="{ width: item.percent + '%', backgroundColor: item.color }">
                            <span class="p-bar-text" :class="{ 'text-outside': item.percent < 15 }">{{ item.percent > 0 ? item.percent + '%' : '' }}</span>
                         </div>
                       </div>
                     </div>
                   </div>
                </div>
              </div>

              <!-- Activity Overview (New) -->
              <div class="chart-card activity-overview-card">
                <div class="chart-header">
                  <div style="display: flex; justify-content: space-between; align-items: center;">
                    <h4>Activity Overview</h4>
                    <el-select v-model="activityRange" size="small" class="dark-mini-select" style="width: 110px;">
                      <el-option label="Last 7 days" value="7d" />
                      <el-option label="Last 30 days" value="30d" />
                    </el-select>
                  </div>
                </div>
                <div class="chart-body">
                  <div id="activity-line-chart" style="width: 100%; height: 250px;"></div>
                </div>
              </div>

              <!-- Upcoming Schedule (New) -->
              <div class="chart-card schedule-card">
                <div class="chart-header">
                  <div style="display: flex; justify-content: space-between; align-items: center;">
                    <h4>Upcoming Schedule</h4>
                    <div class="schedule-nav">
                      <i class="fa-solid fa-chevron-left"></i>
                      <i class="fa-solid fa-chevron-right"></i>
                    </div>
                  </div>
                </div>
                <div class="chart-body schedule-body">
                  <div class="days-indicator">
                    <div class="day-col" v-for="d in weekDays" :key="d.date">
                      <span class="day-name">{{ d.name }}</span>
                      <div class="day-num" :class="{ active: d.isToday }">{{ d.day }}</div>
                    </div>
                  </div>
                  <div class="schedule-items">
                    <div class="empty-state">
                      <p>No upcoming events</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- TAB CONTENT: BOARD (Kanban) -->
          <div class="board-content" v-if="currentTab === 'board'">
            <div class="board-toolbar">
              <div class="board-search">
                <i class="fa-solid fa-magnifying-glass"></i>
                <input type="text" placeholder="Tìm kiếm trong bảng..." />
              </div>
              <div class="board-avatars">
                <div class="avatar-stack">
                  <div class="stack-item" style="background: #3b82f6;">DN</div>
                  <div class="stack-item" style="background: #a855f7;">AN</div>
                  <div class="stack-item" style="background: #f59e0b;">TN</div>
                  <div class="stack-count">+0</div>
                </div>
              </div>
              <el-button size="small" plain class="dark-btn"><i class="fa-solid fa-filter"></i> Bộ lọc</el-button>
              <div class="board-grouping">
                <el-popover placement="bottom-start" :width="400" trigger="click" popper-class="group-popover">
                    <template #reference>
                        <el-button size="small" plain class="dark-btn">Group <i class="fa-solid fa-chevron-down"></i></el-button>
                    </template>
                    <div class="group-popover-content">
                        <div class="popover-section-label">Nhóm theo</div>
                        <div class="group-config-row">
                        <el-select v-model="groupBy" placeholder="Chọn trường" class="group-select-field">
                            <el-option label="Trạng thái" value="status">
                            <i class="fa-solid fa-circle-nodes" style="margin-right: 8px;"></i> Trạng thái
                            </el-option>
                            <el-option label="Người thực hiện" value="assignee">
                            <i class="fa-solid fa-user" style="margin-right: 8px;"></i> Người thực hiện
                            </el-option>
                            <el-option label="Độ ưu tiên" value="priority">
                            <i class="fa-solid fa-flag" style="margin-right: 8px;"></i> Độ ưu tiên
                            </el-option>
                        </el-select>
                        <el-select v-model="groupByOrder" placeholder="Thứ tự" class="group-select-order">
                            <el-option label="Tăng dần" value="asc" />
                            <el-option label="Giảm dần" value="desc" />
                        </el-select>
                        </div>
                    </div>
                </el-popover>
              </div>
              <el-button v-if="isSprintActive" size="small" type="primary" class="sprint-btn" @click="openCompleteSprintModal">Complete sprint</el-button>
            </div>

                          <KanbanBoard 
              :tasks="tasks"
              :statuses="['TO DO', 'IN PROGRESS', 'IN REVIEW', 'DONE']"
              :projectId="projectId"
              :loading="isFetchingTasks"
              @task-click="openTaskDetail"
              @status-changed="(payload) => handleDraggableChange(null, payload)"
              @task-reordered="handleTaskReordered"
              @refresh="fetchTasks"
            />

            </div>

          <!-- TAB CONTENT: BACKLOG (List) -->
          <div class="backlog-content" v-if="currentTab === 'backlog'">
            <div class="backlog-header-jira">
               <h2 class="backlog-title">Tồn đọng</h2>
               <p class="muted-text">Kéo thả các công việc để ưu tiên hoặc thay đổi trạng thái.</p>
            </div>
            <div class="backlog-list-container">
              <div v-for="group in backlogTaskGroups" :key="group.id" class="backlog-group">
                <div class="backlog-group-header" :style="{ borderLeft: `4px solid ${group.statusBg}` }">
                  <i class="fa-solid fa-chevron-down"></i>
                  <span class="bg-header-text">{{ group.statusText }}</span>
                  <span class="bg-header-count">{{ group.items.length }}</span>
                </div>
                <draggable 
                  class="backlog-items-area" 
                  :list="group.items" 
                  group="tasks" 
                  item-key="id"
                  @change="(evt) => handleDraggableChange(evt, group)"
                >
                  <template #item="{ element }">
                    <div class="backlog-item-row" @click="openTaskDetail(element)">
                       <div class="bi-left">
                          <i class="fa-solid fa-square-check" :style="{ color: element.typeName === 'Bug' ? '#ef4444' : '#3b82f6' }"></i>
                          <span class="bi-key">{{ element.id.substring(0, 8).toUpperCase() }}</span>
                          <span class="bi-title">{{ element.title }}</span>
                       </div>
                       <div class="bi-right">
                          <span class="prio-tag" :class="'prio-' + element.priority">{{ element.priority }}</span>
                          <div class="avatar-circle-xs">{{ element.reporterName?.substring(0, 2).toUpperCase() || '?' }}</div>
                       </div>
                    </div>
                  </template>
                </draggable>
              </div>
            </div>
          </div>

          <!-- TAB CONTENT: CALENDAR -->
          <div class="calendar-content" v-if="currentTab === 'calendar'">
            <div class="calendar-header-toolbar">
              <h2 class="calendar-month-title">Tháng 3, 2026</h2>
              <div class="calendar-nav-controls">
                <div class="btn-group-jira">
                  <el-button size="small" class="jira-control-btn"><i class="fa-solid fa-chevron-left"></i></el-button>
                  <el-button size="small" class="jira-control-btn today-btn">Hôm nay</el-button>
                  <el-button size="small" class="jira-control-btn"><i class="fa-solid fa-chevron-right"></i></el-button>
                </div>
              </div>
              <div class="calendar-view-toggle">
                <div class="toggle-group-jira">
                  <div class="toggle-item active">Tháng</div>
                  <div class="toggle-item">Tuần</div>
                  <div class="toggle-item">Ngày</div>
                </div>
              </div>
            </div>

            <div class="calendar-grid-container">
              <!-- Weekday Headers -->
              <div class="calendar-week-labels">
                <div class="weekday-label">CN</div>
                <div class="weekday-label">THỨ 2</div>
                <div class="weekday-label">THỨ 3</div>
                <div class="weekday-label">THỨ 4</div>
                <div class="weekday-label">THỨ 5</div>
                <div class="weekday-label">THỨ 6</div>
                <div class="weekday-label">THỨ 7</div>
              </div>

              <!-- Days Grid -->
              <div class="calendar-days-grid">
                <!-- Row 1 -->
                <div class="day-cell muted">24</div>
                <div class="day-cell muted">25</div>
                <div class="day-cell muted">26</div>
                <div class="day-cell muted">27</div>
                <div class="day-cell muted">28</div>
                <div class="day-cell">1</div>
                <div class="day-cell">2</div>

                <!-- Row 2 -->
                <div class="day-cell">3</div>
                <div class="day-cell">4</div>
                <div class="day-cell">5</div>
                <div class="day-cell">6</div>
                <div class="day-cell">7</div>
                <div class="day-cell">8</div>
                <div class="day-cell">9</div>

                <!-- Row 3 -->
                <div class="day-cell">10</div>
                <div class="day-cell">11</div>
                <div class="day-cell today">
                  <span class="day-num-circle">12</span>
                </div>
                <div class="day-cell">13</div>
                <div class="day-cell">14</div>
                <div class="day-cell">15</div>
                <div class="day-cell">16</div>

                <!-- Row 4 -->
                <div class="day-cell">17</div>
                <div class="day-cell">18</div>
                <div class="day-cell">19</div>
                <div class="day-cell">20</div>
                <div class="day-cell">21</div>
                <div class="day-cell">22</div>
                <div class="day-cell">23</div>
              </div>
            </div>
          </div>

          <!-- TAB CONTENT: TIMELINE (ROADMAP) -->
          <div class="timeline-content" v-if="currentTab === 'timeline'">
            <div class="timeline-toolbar">
              <div class="timeline-search">
                <i class="fa-solid fa-magnifying-glass"></i>
                <input type="text" placeholder="Tìm kiếm trong lộ trình..." />
              </div>
              <div class="avatar-circle-sm">DN</div>
              <el-button size="small" plain class="dark-btn">Danh mục trạng thái <i class="fa-solid fa-chevron-down"></i></el-button>
              <div class="toolbar-right-icons">
                <i class="fa-solid fa-sliders"></i>
                <i class="fa-solid fa-ellipsis"></i>
              </div>
            </div>

            <div class="timeline-grid-wrapper">
              <div class="timeline-left-panel">
                <div class="panel-header">Công việc</div>
                <div class="panel-sub-header">Sprints</div>
                <div class="panel-list">
                  <div class="add-epic-btn" v-if="canEditBoard"><i class="fa-solid fa-plus"></i> Tạo Epic</div>
                </div>
              </div>
              
              <div class="timeline-main-grid">
                <!-- Timeline Headers -->
                <div class="timeline-months-row">
                  <div class="month-col">Tháng 3 '26</div>
                  <div class="month-col">Tháng 4 '26</div>
                  <div class="month-col">Tháng 5 '26</div>
                  <div class="month-col">Tháng 6 '26</div>
                  <div class="month-col">Tháng 7 '26</div>
                </div>
                
                <!-- Timeline Grid Lines -->
                <div class="timeline-rows-container">
                  <div class="timeline-task-row" v-for="i in 4" :key="'row-'+i">
                    <div class="grid-line" v-for="j in 5" :key="'line-'+j"></div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Bottom Navigation -->
            <div class="timeline-bottom-nav">
              <div class="bottom-nav-group">
                <div class="bottom-item">Hôm nay</div>
                <div class="bottom-item">Tuần</div>
                <div class="bottom-item active">Tháng</div>
                <div class="bottom-item">Quý</div>
              </div>
              <div class="bottom-nav-icons">
                <i class="fa-solid fa-circle-info"></i>
                <i class="fa-solid fa-chevron-right"></i>
              </div>
            </div>
          </div>

          <!-- TAB CONTENT: PAGE (KNOWLEDGE BASE) -->
          <div class="pages-content" v-if="currentTab === 'page'">
            <div class="pages-layout-wrapper">
              <div class="pages-main-area">
                <div class="pages-header-section">
                  <h1 class="pages-title">Trang <span class="try-badge">Dùng thử</span></h1>
                  <p class="pages-desc">Ghi lại kiến thức của nhóm và cải thiện cách bạn hoàn thành công việc.</p>
                </div>

                <!-- Browser-like Editor Window -->
                <div class="editor-window">
                  <div class="editor-window-header">
                    <div class="header-dots">
                      <span></span><span></span><span></span>
                    </div>
                  </div>
                  <div class="editor-toolbar-expanded">
                    <div class="t-left">
                      <div class="t-icon"><i class="fa-solid fa-rotate-left"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-rotate-right"></i></div>
                      <div class="t-divider"></div>
                      <div class="t-dropdown">Văn bản thường <i class="fa-solid fa-chevron-down"></i></div>
                      <div class="t-divider"></div>
                      <div class="t-icon active"><i class="fa-solid fa-bold"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-italic"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-ellipsis"></i></div>
                      <div class="t-divider"></div>
                      <div class="t-icon"><i class="fa-solid fa-align-left"></i></div>
                      <div class="t-divider"></div>
                      <div class="t-icon"><i class="fa-solid fa-font"></i> <i class="fa-solid fa-chevron-down" style="font-size: 8px;"></i></div>
                      <div class="t-divider"></div>
                      <div class="t-icon"><i class="fa-solid fa-list-ul"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-list-check"></i></div>
                      <div class="t-divider"></div>
                      <div class="t-icon"><i class="fa-solid fa-image"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-link"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-at"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-face-smile"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-plus"></i> <i class="fa-solid fa-chevron-down" style="font-size: 8px;"></i></div>
                    </div>
                    <div class="t-right">
                      <span class="saving-text">Đang lưu...</span>
                      <div class="t-avatar-circle">D</div>
                      <div class="t-icon"><i class="fa-solid fa-magnifying-glass"></i></div>
                      <div class="t-icon"><i class="fa-solid fa-circle-question"></i></div>
                      <el-button type="primary" size="small" class="publish-btn">Xuất bản</el-button>
                      <el-button size="small" plain class="close-btn">Đóng</el-button>
                      <div class="t-icon"><i class="fa-solid fa-ellipsis"></i></div>
                    </div>
                  </div>
                  <div class="editor-content-area">
                    <h2 class="editor-main-heading">Bắt đầu từ đầu với một trang trống</h2>
                    <p class="editor-main-p">Trang là nơi để nắm bắt tất cả các thông tin quan trọng của bạn. Bắt đầu với một trang trống và thêm các nội dung phong phú như công việc, hình ảnh, macro, các vấn đề của Jira Software và lộ trình.</p>
                    <p class="editor-tip"><span class="mention-tag">@nhắc_tên</span> để gắn thẻ đồng đội của bạn và cộng tác với việc cùng chỉnh sửa và bình luận trực tiếp.</p>
                  </div>
                </div>

                <!-- Footer Bar -->
                <div class="pages-footer-bar">
                  <span>Mở khóa các mẫu, trang dự án và nhiều hơn nữa.</span>
                  <div class="footer-actions">
                    <span class="learn-more">Tìm hiểu thêm</span>
                    <el-button type="primary" size="small" class="try-now-btn">Thử ngay</el-button>
                  </div>
                </div>
              </div>

              <div class="pages-template-sidebar">
                <h3 class="template-sidebar-title">Xem trước mẫu</h3>
                <p class="template-sidebar-subtitle">PHỔ BIẾN VỚI CÁC NHÓM NHƯ BẠN</p>
                <div class="template-list">
                  <div class="template-item active">
                    <div class="template-icon blue"><i class="fa-regular fa-file-lines"></i></div>
                    <div class="template-info">
                      <div class="template-name">Trang trống</div>
                      <div class="template-desc">Bắt đầu một trang từ đầu.</div>
                    </div>
                  </div>
                  <div class="template-item">
                    <div class="template-icon purple"><i class="fa-solid fa-list-check"></i></div>
                    <div class="template-info">
                      <div class="template-name">Yêu cầu sản phẩm</div>
                      <div class="template-desc">Xác định, theo dõi và phạm vi yêu cầu.</div>
                    </div>
                  </div>
                  <div class="template-item">
                    <div class="template-icon green"><i class="fa-solid fa-code-pull-request"></i></div>
                    <div class="template-info">
                      <div class="template-name">Quyết định</div>
                      <div class="template-desc">Ghi lại các quyết định dự án quan trọng.</div>
                    </div>
                  </div>
                  <div class="template-item">
                    <div class="template-icon light-blue"><i class="fa-solid fa-users"></i></div>
                    <div class="template-info">
                      <div class="template-name">Ghi chú cuộc họp</div>
                      <div class="template-desc">Lập chương trình họp, ghi chép hành động.</div>
                    </div>
                  </div>
                  <div class="template-item">
                    <div class="template-icon yellow"><i class="fa-solid fa-rotate-left"></i></div>
                    <div class="template-info">
                      <div class="template-name">Họp tổng kết</div>
                      <div class="template-desc">Điều gì đã tốt? Điều gì có thể tốt hơn?</div>
                    </div>
                  </div>
                </div>
                <div class="explore-more">
                  <i class="fa-regular fa-compass"></i> Khám phá thêm nhiều mẫu
                </div>
              </div>
            </div>
          </div>

          <!-- TAB CONTENT: FORMS (INTAKE) -->
          <div class="forms-content" v-if="currentTab === 'forms'">
            <div class="forms-hero">
              <h1 class="forms-headline">Cách đơn giản để thu thập và theo dõi yêu cầu công việc</h1>
              <p class="forms-subheadline">Sử dụng biểu mẫu để tạo công việc từ các yêu cầu của bên liên quan một cách liền mạch và tự động hóa quy trình tiếp nhận của bạn.</p>
            </div>

            <div class="forms-workflow-visual">
              <div class="visual-card">
                <div class="visual-icon"><i class="fa-regular fa-file-lines"></i></div>
                <div class="visual-label">Gửi biểu mẫu</div>
              </div>
              <div class="visual-arrow"><i class="fa-solid fa-arrow-right"></i></div>
              <div class="visual-card">
                <div class="visual-icon active"><i class="fa-regular fa-square-check"></i></div>
                <div class="visual-label">Theo dõi yêu cầu</div>
              </div>
            </div>

            <div class="forms-features-grid">
              <div class="feature-card">
                <h3 class="feature-title">Lưu lại các chi tiết quan trọng</h3>
                <p class="feature-desc">Thiết kế biểu mẫu tùy chỉnh với nhiều loại trường khác nhau để lấy thông tin mà nhóm của bạn cần để bắt đầu ngay lập tức.</p>
              </div>
              <div class="feature-card">
                <h3 class="feature-title">Ưu tiên công việc</h3>
                <p class="feature-desc">Mọi lượt gửi biểu mẫu sẽ tự động tạo một công việc mới trong dự án của bạn, sẵn sàng để nhóm của bạn giải quyết.</p>
              </div>
            </div>

            <div class="forms-cta-section">
              <el-button type="primary" class="create-form-btn"><i class="fa-solid fa-plus"></i> Tạo biểu mẫu</el-button>
            </div>
          </div>


          <div class="list-tab-wrapper" v-if="currentTab === 'list'">
            
            <div class="list-toolbar">
              <div class="toolbar-left">
                <!-- Redesigned Group Button (Image 3 style) -->
                <el-popover placement="bottom-start" :width="400" trigger="click" popper-class="group-popover">
                  <template #reference>
                    <div class="toolbar-btn group-active-btn">
                      <i class="fa-solid fa-layer-group"></i> 
                      Nhóm: {{ 
                        groupBy === 'status' ? 'Trạng thái' : 
                        groupBy === 'assignee' ? 'Người thực hiện' :
                        groupBy === 'priority' ? 'Độ ưu tiên' :
                        groupBy === 'type' ? 'Loại công việc' :
                        groupBy === 'dueDate' ? 'Ngày hết hạn' : 'Trạng thái'
                      }}
                    </div>
                  </template>
                  <div class="group-popover-content">
                    <div class="popover-section-label">Nhóm theo</div>
                    <div class="group-config-row">
                      <el-select v-model="groupBy" placeholder="Chọn trường" class="group-select-field">
                        <el-option label="Trạng thái" value="status">
                           <i class="fa-solid fa-circle-nodes" style="margin-right: 8px;"></i> Trạng thái
                        </el-option>
                        <el-option label="Người thực hiện" value="assignee">
                           <i class="fa-solid fa-user" style="margin-right: 8px;"></i> Người thực hiện
                        </el-option>
                        <el-option label="Độ ưu tiên" value="priority">
                           <i class="fa-solid fa-flag" style="margin-right: 8px;"></i> Độ ưu tiên
                        </el-option>
                        <el-option label="Nhãn" value="tags">
                           <i class="fa-solid fa-tag" style="margin-right: 8px;"></i> Nhãn
                        </el-option>
                        <el-option label="Ngày hết hạn" value="dueDate">
                           <i class="fa-solid fa-calendar" style="margin-right: 8px;"></i> Ngày hết hạn
                        </el-option>
                        <el-option label="Loại công việc" value="type">
                           <i class="fa-solid fa-cube" style="margin-right: 8px;"></i> Loại công việc
                        </el-option>
                      </el-select>

                      <el-select v-model="groupByOrder" placeholder="Thứ tự" class="group-select-order">
                        <el-option label="Tăng dần" value="asc" />
                        <el-option label="Giảm dần" value="desc" />
                      </el-select>

                      <div class="group-action-icons">
                        <i class="fa-solid fa-trash-can delete-group-icon"></i>
                      </div>
                    </div>
                    <div class="group-footer-row">
                       <el-switch v-model="showEmptyGroups" size="small" />
                       <span class="footer-label">Hiện các nhóm trống</span>
                    </div>
                  </div>
                </el-popover>
              </div>
              <div class="toolbar-right">
                <el-dropdown trigger="click" @command="handleFilterCommand">
                  <div class="toolbar-btn" :class="{ 'primary-tint': activeFilters.priority }">
                    <i class="fa-solid fa-filter"></i> Bộ lọc
                  </div>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="clear">Xóa bộ lọc</el-dropdown-item>
                      <el-dropdown-item divided command="priority:1">Độ ưu tiên: Khẩn cấp (1)</el-dropdown-item>
                      <el-dropdown-item command="priority:2">Độ ưu tiên: Cao (2)</el-dropdown-item>
                      <el-dropdown-item command="priority:3">Độ ưu tiên: Trung bình (3)</el-dropdown-item>
                      <el-dropdown-item command="priority:4">Độ ưu tiên: Thấp (4)</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                
                <div class="toolbar-btn" @click="toggleShowCompleted" :class="{ 'primary-tint': !showCompleted }">
                  <i :class="showCompleted ? 'fa-regular fa-circle-check' : 'fa-solid fa-circle-check'"></i>
                  {{ showCompleted ? 'Hoàn thành' : 'Đang thực hiện' }}
                </div>

                <el-dropdown trigger="click" @command="(val) => activeFilters.assigneeName = (val === 'all' ? null : val)">
                  <div class="toolbar-btn" :class="{ 'primary-tint': activeFilters.assigneeName }">
                    <i class="fa-solid fa-user-plus"></i> {{ activeFilters.assigneeName || 'Người thực hiện' }} 
                    <div class="avatar-tiny" v-if="activeFilters.assigneeName">{{ activeFilters.assigneeName[0] }}</div>
                  </div>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="all">Tất cả</el-dropdown-item>
                      <el-dropdown-item divided command="Danh Nguyễn">Danh Nguyễn</el-dropdown-item>
                      <el-dropdown-item command="Admin">Admin</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>

                <div class="toolbar-icon"><i class="fa-solid fa-magnifying-glass"></i></div>
                <el-dropdown trigger="click" @command="handleSortCommand">
                   <div class="toolbar-btn" :class="{ 'primary-tint': sortBy }">
                     <i class="fa-solid fa-arrow-down-wide-short"></i> {{ sortBy ? `Sắp xếp: ${sortBy}` : 'Sắp xếp' }}
                   </div>
                   <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="title">Theo tên</el-dropdown-item>
                      <el-dropdown-item command="date">Theo ngày hết hạn</el-dropdown-item>
                      <el-dropdown-item command="priority">Theo độ ưu tiên</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <button class="add-task-white-btn" @click="openCreateTask">Thêm công việc <i class="fa-solid fa-chevron-down" style="font-size:10px; margin-left:6px;"></i></button>
              </div>
            </div>

            <div class="list-view-container">
              <div v-for="group in taskGroups" :key="group.id" class="task-group" :style="group.statusText === 'IN PROGRESS' && group.items.length > wipLimit ? 'border: 2px solid #ef4444; border-radius: 6px; position: relative;' : ''">
                <!-- Group Header -->
                <div class="group-header sprint-like-header" style="background-color: var(--bg-layout); padding: 12px 16px; border-bottom: 1px solid var(--border-color); display: flex; justify-content: space-between; align-items: center;">
                  <div class="sprint-header-left" style="display: flex; align-items: center; gap: 8px; flex: 1;">
                    <el-checkbox style="margin-right: 4px;" @click.stop></el-checkbox>
                    <i class="fa-solid fa-chevron-down toggle-icon" 
                       @click="group.expanded = !group.expanded" 
                       :style="{ transform: group.expanded ? 'rotate(0)' : 'rotate(-90deg)', fontSize: '12px', color: '#64748b', cursor: 'pointer' }"></i>
                       
                    <span class="sprint-header-title" style="font-weight: 600; font-size: 14px; color: var(--text-primary); margin-left: 4px;">
                      {{ group.statusText }}
                    </span>
                    
                    <span class="sprint-header-meta" style="color: #64748b; font-size: 12px; margin-left: 6px;">
                      <span v-if="group.startDate && group.endDate">{{ formatDateShort(group.startDate) }} - {{ formatDateShort(group.endDate) }} &nbsp;</span>
                      ({{ group.items.length }} work items)
                    </span>
                    <span v-if="group.statusText === 'IN PROGRESS' && group.items.length > wipLimit" style="color: #ef4444; font-size: 12px; margin-left: 12px; font-weight: 500;">
                      <i class="fa-solid fa-triangle-exclamation"></i> Vượt giới hạn công việc tối ưu
                    </span>
                  </div>

                  <!-- Start Sprint Controls -->
                  <div class="group-sprint-actions" v-if="taskGroups.indexOf(group) === 0" style="display: flex; align-items: center; gap: 8px;">
                     <div class="sprint-stats-badges" style="display: flex; gap: 4px; margin-right: 8px;">
                       <el-tooltip content="To Do Story Points" placement="top">
                         <span class="stat-badge stat-todo" style="background: #e2e8f0; color: #475569; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 600;">{{ getStatusStoryPoints(group, 'TODO') }}</span>
                       </el-tooltip>
                       <el-tooltip content="In Progress Story Points" placement="top">
                         <span class="stat-badge stat-progress" style="background: #bfdbfe; color: #1e40af; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 600;">{{ getStatusStoryPoints(group, 'IN PROGRESS') }}</span>
                       </el-tooltip>
                       <el-tooltip content="Done Story Points" placement="top">
                         <span class="stat-badge stat-done" style="background: #dcfce7; color: #166534; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 600;">{{ getStatusStoryPoints(group, 'DONE') }}</span>
                       </el-tooltip>
                     </div>
                     <el-button v-if="isSprintActive" size="small" class="sprint-action-btn" @click.stop="openCompleteSprintModal" style="color: #1e293b; font-weight: 500;">Complete sprint</el-button>
                     <el-button v-else size="small" class="sprint-action-btn" @click.stop="openStartSprintModal" style="color: #1e293b; font-weight: 500;">Start sprint</el-button>
                     
                     <el-dropdown trigger="click" @command="handleSprintMenu">
                       <div class="sprint-more-btn" style="width: 28px; height: 28px; border-radius: 4px; border: 1px solid #3b82f6; display: flex; align-items: center; justify-content: center; cursor: pointer; color: #3b82f6; background: rgba(59, 130, 246, 0.1);">
                         <i class="fa-solid fa-ellipsis"></i>
                       </div>
                       <template #dropdown>
                         <el-dropdown-menu class="dark-dropdown">
                           <el-dropdown-item command="reorder">Reorder work items</el-dropdown-item>
                           <el-dropdown-item @click="openBurndownChart(group.sprintId)">Sprint Burndown</el-dropdown-item>
                           <el-dropdown-item command="edit">Edit sprint</el-dropdown-item>
                           <el-dropdown-item command="delete">Delete sprint</el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>
                  </div>
                </div>

                <!-- Group Content -->
                <div class="group-content" v-show="group.expanded">
                  <!-- Draggable Container -->
                  <draggable 
                    class="draggable-list-content" 
                    :list="group.items" 
                    group="tasks" 
                    item-key="id"
                    @change="(evt) => handleDraggableChange(evt, group)"
                  >
                    <template #item="{ element: task }">
                      <div class="task-node-wrapper">
                        <div class="list-row task-row sprint-task-row" style="display: flex; align-items: center; justify-content: space-between; padding: 8px 16px; border-bottom: 1px solid var(--border-color); background: var(--bg-nav); cursor: pointer; transition: background 0.2s;">
                          <div class="task-row-left" style="display: flex; align-items: center; gap: 12px; flex: 1;" @click="openTaskDetail(task)">
                            <el-checkbox @click.stop style="margin-right: 4px;"></el-checkbox>
                            
                            <!-- Priority Icon -->
                            <div class="priority-trigger" @click.stop="() => {}">
                              <el-dropdown trigger="click" @command="(val) => updateTaskField(task, 'priority', val)">
                                <span style="font-size: 13px;">
                                  <i class="fa-solid fa-angles-up" v-if="task.priority === 1" style="color: #ef4444;" title="Urgent"></i>
                                  <i class="fa-solid fa-angle-up" v-else-if="task.priority === 2" style="color: #f97316;" title="High"></i>
                                  <i class="fa-solid fa-minus" v-else-if="task.priority === 3" style="color: #3b82f6;" title="Normal"></i>
                                  <i class="fa-solid fa-angle-down" v-else style="color: #94a3b8;" title="Low"></i>
                                </span>
                                <template #dropdown>
                                  <el-dropdown-menu class="dark-dropdown">
                                    <el-dropdown-item :command="1">1 (Urgent)</el-dropdown-item>
                                    <el-dropdown-item :command="2">2 (High)</el-dropdown-item>
                                    <el-dropdown-item :command="3">3 (Normal)</el-dropdown-item>
                                    <el-dropdown-item :command="4">4 (Low)</el-dropdown-item>
                                  </el-dropdown-menu>
                                </template>
                              </el-dropdown>
                            </div>
                            
                            <span class="task-id" style="font-family: monospace; font-size: 13px; color: #94a3b8; font-weight: 500; display: flex; align-items: center; gap: 6px;">
                              <i class="fa-solid fa-square-check" style="color: #3b82f6;"></i>
                              {{ task.id.substring(0, 8).toUpperCase() }}
                            </span>
                            
                            <span class="task-title" style="font-size: 14px; font-weight: 500; color: var(--text-primary);">{{ task.title }}</span>
                          </div>
                          
                          <div class="task-row-right" style="display: flex; align-items: center; gap: 20px;">
                            <!-- Comments bubble -->
                            <el-popover placement="bottom-end" :width="440" trigger="click" popper-class="comment-popover-dark">
                              <template #reference>
                                <div class="comment-trigger-btn" @click.stop v-if="task.commentCount" style="display: flex; align-items: center; gap: 4px; color: #94a3b8; cursor: pointer;">
                                  <i class="fa-regular fa-comment icon-btn" style="font-size: 13px;"></i>
                                  <span style="font-size: 11px; font-weight: 700;">{{ task.commentCount }}</span>
                                </div>
                              </template>
                              <div class="comment-popover-content">
                                  <div class="comments-scroll-area">Bình luận...</div>
                              </div>
                            </el-popover>
                          
                            <!-- Story Point badge -->
                            <div class="story-point-badge" v-if="task.storyPointEstimate || task.storyPoints" style="background: #334155; color: #cbd5e1; font-size: 11px; padding: 2px 8px; border-radius: 12px; font-weight: 600;" title="Story Points">
                              {{ task.storyPointEstimate || task.storyPoints }}
                            </div>
                            
                            <!-- Branch icon -->
                            <el-tooltip content="Create Branch" placement="top">
                              <i class="fa-solid fa-code-branch" style="color: #64748b; font-size: 13px; cursor: pointer;"></i>
                            </el-tooltip>
                            
                            <!-- Status Dropdown matching UI -->
                            <el-dropdown trigger="click" @command="(val) => updateTaskField(task, 'statusName', val)">
                              <div class="sprint-status-btn" :style="{ backgroundColor: task.statusName === 'DONE' ? '#166534' : group.statusBg || '#334155', color: group.statusColor || '#ffffff' }" style="display: flex; align-items: center; gap: 6px; padding: 4px 10px; border-radius: 4px; font-size: 11px; font-weight: 700; cursor: pointer;">
                                <span>{{ (task.statusName || 'TODO').toUpperCase() }}</span>
                                <i class="fa-solid fa-chevron-down" style="font-size: 10px;"></i>
                              </div>
                              <template #dropdown>
                                <el-dropdown-menu class="dark-dropdown">
                                  <el-dropdown-item command="TO DO">TO DO</el-dropdown-item>
                                  <el-dropdown-item command="IN PROGRESS">IN PROGRESS</el-dropdown-item>
                                  <el-dropdown-item command="DONE">DONE</el-dropdown-item>
                                </el-dropdown-menu>
                              </template>
                            </el-dropdown>
                            
                            <!-- Assignee Avatar -->
                            <el-dropdown trigger="click" @command="(val) => updateTaskField(task, 'assignedUserId', val)">
                              <div class="assignee-trigger" style="width: 24px; height: 24px; border-radius: 50%; background: #334155; display: flex; align-items: center; justify-content: center; cursor: pointer;">
                                <div class="avatar-tiny" v-if="task.assigneeName" style="background: transparent; color: white;">{{ task.assigneeName.substring(0, 2).toUpperCase() }}</div>
                                <i class="fa-solid fa-user-plus icon-btn" v-else style="font-size: 10px; color: #94a3b8;"></i>
                              </div>
                              <template #dropdown>
                                <el-dropdown-menu class="dark-dropdown">
                                  <el-dropdown-item :command="null">Chưa phân công</el-dropdown-item>
                                  <el-dropdown-item v-for="member in projectMembers" :key="member.userId" :command="member.userId">
                                    {{ member.fullName }}
                                  </el-dropdown-item>
                                </el-dropdown-menu>
                              </template>
                            </el-dropdown>
                          </div>
                        </div>

                        <!-- Subtasks Container -->
                        <div v-if="task.showChildren || task.childTasks?.length > 0" class="subtasks-container" style="background: rgba(0,0,0,0.15);">
                           <div v-for="subtask in task.childTasks" :key="subtask.id" class="subtask-row" style="display: flex; align-items: center; justify-content: space-between; padding: 6px 16px 6px 48px; border-bottom: 1px solid var(--border-color); font-size: 13px;" @click="openTaskDetail(subtask)">
                               <div style="display: flex; align-items: center; gap: 8px;">
                                   <i class="fa-solid fa-level-up-alt fa-rotate-90" style="color: #64748b; margin-right: 4px;"></i>
                                   <span class="task-id" style="font-family: monospace; color: #94a3b8;">{{ subtask.id.substring(0, 8).toUpperCase() }}</span>
                                   <span style="color: var(--text-primary);">{{ subtask.title }}</span>
                               </div>
                               <!-- Subtask status/actions -->
                               <div style="display: flex; align-items: center; gap: 12px;">
                                    <!-- Assignee Avatar -->
                                    <div class="assignee-trigger" style="width: 20px; height: 20px; border-radius: 50%; background: #334155; display: flex; align-items: center; justify-content: center;">
                                      <div class="avatar-tiny" v-if="subtask.assigneeName" style="background: transparent; color: white;">{{ subtask.assigneeName.substring(0, 2).toUpperCase() }}</div>
                                      <i class="fa-solid fa-user icon-btn" v-else style="font-size: 9px; color: #94a3b8;"></i>
                                    </div>
                                    <div class="sprint-status-btn" style="padding: 2px 8px; border-radius: 4px; font-size: 10px; background: #334155; color: #fff;">
                                      {{ (subtask.statusName || 'TODO').toUpperCase() }}
                                    </div>
                               </div>
                           </div>
                           
                           <!-- Add Subtask Input -->
                           <div class="add-subtask-row" style="padding: 6px 16px 6px 48px; display: flex; align-items: center; border-bottom: 1px solid var(--border-color);">
                                <i class="fa-solid fa-plus" style="font-size: 10px; color: #64748b; margin-right: 8px;"></i>
                                <input type="text" placeholder="Thêm subtask..." v-model="task.newSubtaskTitle" @keyup.enter="createSubtask(task)" style="background: transparent; border: none; outline: none; color: var(--text-primary); font-size: 13px; width: 100%;" />
                           </div>
                        </div>

                      </div>
                    </template>
                  </draggable>
                </div> <!-- Closes group-content -->

                <!-- Add Task Row -->
                <div class="list-row add-task-row" v-if="!group.showQuickAdd" @click="openCreateTask(group.statusText)">
                  <div class="col-name">
                    <i class="fa-solid fa-plus" style="font-size: 12px; margin-right: 8px;"></i> Thêm công việc
                  </div>
                </div>

                <!-- Quick Add Input Row -->
                <div class="list-row quick-add-input-row" v-else>
                  <div class="col-name col-full-width">
                    <input 
                      type="text" 
                      class="quick-add-input" 
                      placeholder="Công việc mới..." 
                      v-model="group.quickAddTitle" 
                      @keyup.enter="createQuickTask(group)"
                      @blur="toggleQuickAdd(group)"
                      ref="quickAddInput"
                    />
                  </div>
                </div>
              </div> <!-- Closes task-group (Line 774) -->
            </div> <!-- Closes list-view-container (Line 773) -->
          </div> <!-- Closes list-tab-wrapper (Line 703) -->
        <!-- Main Layout tags handled by Nexus Layout -->

      <!-- Members Management Dialog -->
      <el-dialog
        v-model="showTeamsDialog"
        title="Thành viên dự án"
        width="560px"
        custom-class="jira-dark-dialog"
      >
        <div class="members-dialog-body" style="padding: 10px 0;">
          <!-- Invite Member Section -->
          <div v-if="canManageMembers" class="invite-section" style="margin-bottom: 20px; padding: 16px; background: #161b22; border-radius: 8px; border: 1px solid #30363d;">
            <div style="font-size: 14px; font-weight: 600; color: #f4f5f7; margin-bottom: 12px;"><i class="fa-solid fa-user-plus" style="margin-right: 8px; color: #579dff;"></i>Mời thành viên mới</div>
            <div style="display: flex; gap: 8px; align-items: flex-end;">
              <div style="flex: 1;">
                <label style="font-size: 12px; color: #8c9bab; display: block; margin-bottom: 4px;">Email</label>
                <input v-model="addPeopleEmail" type="email" placeholder="Nhập email thành viên..." style="width: 100%; padding: 8px 12px; background: #22272b; border: 1px solid #30363d; border-radius: 6px; color: #f4f5f7; font-size: 14px; outline: none; box-sizing: border-box;" />
              </div>
              <div style="width: 130px;">
                <label style="font-size: 12px; color: #8c9bab; display: block; margin-bottom: 4px;">Vai trò</label>
                <el-select v-model="addPeopleRole" placeholder="Chọn" size="default" style="width: 100%;">
                  <el-option label="DEV" value="DEV" />
                  <el-option label="QA" value="QA" />
                  <el-option label="PM" value="PM" />
                  <el-option label="PO" value="PO" />
                  <el-option label="SM" value="SM" />
                  <el-option label="Admin" value="Admin" />
                </el-select>
              </div>
              <el-button type="primary" @click="inviteMember" :disabled="!addPeopleEmail" style="height: 34px;"><i class="fa-solid fa-paper-plane" style="margin-right: 6px;"></i>Mời</el-button>
            </div>
          </div>

          <!-- Members List -->
          <div style="font-size: 13px; font-weight: 600; color: #8c9bab; margin-bottom: 10px; text-transform: uppercase; letter-spacing: 0.5px;">Danh sách thành viên ({{ (projectMembers || []).length }})</div>
          <div v-if="isFetchingMembers" class="loading-state" style="text-align: center; color: #8c9bab; padding: 20px;">
             <i class="fa-solid fa-spinner fa-spin"></i> Đang tải danh sách...
          </div>
          <div v-else class="members-list" style="display: flex; flex-direction: column; gap: 8px; max-height: 360px; overflow-y: auto;">
            <div class="member-row" v-for="member in (projectMembers || [])" :key="member.userId" style="display: flex; justify-content: space-between; align-items: center; padding: 10px 14px; border-radius: 8px; background-color: #22272b; transition: background 0.15s;">
              <div class="member-info" style="display: flex; align-items: center; gap: 12px;">
                <div class="avatar-sm" style="background-color: #3b82f6; color: white; width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 13px; flex-shrink: 0;">
                  {{ member.fullName ? member.fullName.substring(0, 2).toUpperCase() : '?' }}
                </div>
                <div class="member-details">
                  <div class="member-name" style="color: #f4f5f7; font-weight: 500; font-size: 14px;">{{ member.fullName }}</div>
                  <div class="member-email" style="color: #8c9bab; font-size: 12px;">{{ member.email }}</div>
                </div>
              </div>
              <div class="member-role" style="display: flex; align-items: center; gap: 8px;">
                <el-dropdown trigger="click" @command="(val) => changeMemberRole(member.userId, val)" v-if="canManageMembers && member.userId !== currentUser.id">
                  <div class="role-trigger" style="color: #579dff; font-size: 13px; font-weight: 500; cursor: pointer; display: flex; align-items: center; gap: 6px; padding: 4px 10px; border-radius: 4px; background: rgba(87, 157, 255, 0.1);">
                    {{ member.projectRole || 'Thành viên' }} <i class="fa-solid fa-chevron-down" style="font-size: 10px;"></i>
                  </div>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="Admin">Admin</el-dropdown-item>
                      <el-dropdown-item command="PM">PM (Quản lý)</el-dropdown-item>
                      <el-dropdown-item command="PO">PO (Product Owner)</el-dropdown-item>
                      <el-dropdown-item command="SM">SM (Scrum Master)</el-dropdown-item>
                      <el-dropdown-item command="DEV">DEV (Lập trình viên)</el-dropdown-item>
                      <el-dropdown-item command="QA">QA (Kiểm thử viên)</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <div v-else class="role-static" style="color: #8c9bab; font-size: 13px; font-weight: 500; padding: 4px 8px;">
                  {{ member.projectRole || 'Thành viên' }}
                </div>
                <!-- (B) Nút Xóa thành viên -->
                <i 
                  v-if="canManageMembers && member.userId !== currentUser.id"
                  class="fa-solid fa-trash-can"
                  style="color: #ef4444; font-size: 13px; cursor: pointer; padding: 6px; border-radius: 4px; transition: background 0.15s;"
                  title="Xóa thành viên khỏi dự án"
                  @click="removeMember(member.userId, member.fullName)"
                ></i>
              </div>
            </div>
            
            <div v-if="(projectMembers || []).length === 0 && !isFetchingMembers" style="text-align: center; color: #8c9bab; padding: 20px;">
              <i class="fa-regular fa-face-meh" style="font-size: 32px; margin-bottom: 8px; display: block; opacity: 0.5;"></i>
              Chưa có thành viên nào trong dự án.
            </div>
          </div>
        </div>
        <template #footer>
          <div class="dialog-footer">
            <el-button @click="showTeamsDialog = false" class="close-btn" style="background: transparent; color: #f4f5f7; border: 1px solid #738496;">Đóng</el-button>
          </div>
        </template>
      </el-dialog>



      <!-- Right AI Sidebar Popup -->
      <transition name="slide-right">
        <aside class="ai-sidebar" v-if="aiVisible">
          <div class="ai-header">
            <h4><i class="fa-solid fa-robot"></i> Trợ lý AI</h4>
            <i class="fa-solid fa-xmark" style="color: #64748b; cursor: pointer" @click="toggleAI"></i>
          </div>
          
          <div class="ai-content">
            <div class="quick-actions-title">THAO TÁC NHANH</div>
            <div class="quick-actions">
              <!-- Dynamically change chips based on tab -->
              <template v-if="currentTab === 'page'">
                <el-button size="small" round plain class="ai-chip">Tóm tắt trang</el-button>
                <el-button size="small" round plain class="ai-chip">Tạo mục hành động</el-button>
                <el-button size="small" round plain class="ai-chip">Sửa giọng văn & ngữ pháp</el-button>
              </template>
              <template v-else-if="currentTab === 'forms'">
                <el-button size="small" round plain class="ai-chip">Tạo biểu mẫu</el-button>
                <el-button size="small" round plain class="ai-chip">Tóm tắt</el-button>
              </template>
              <template v-else-if="currentTab === 'timeline'">
                <el-button size="small" round plain class="ai-chip">Tóm tắt lộ trình</el-button>
                <el-button size="small" round plain class="ai-chip">Gợi ý mốc thời gian</el-button>
              </template>
              <template v-else>
                <el-button size="small" round plain class="ai-chip">Tóm tắt bảng</el-button>
                <el-button size="small" round plain class="ai-chip">Gợi ý thứ tự công việc</el-button>
                <el-button size="small" round plain class="ai-chip">Kiểm tra phụ thuộc</el-button>
              </template>
            </div>

            <div class="chat-message bot">
              <div class="avatar-bot"><i class="fa-solid fa-robot"></i></div>
              <div class="message-bubble">
                Chào bạn! Tôi có thể giúp bạn sắp xếp Không gian nhóm. Bạn có muốn tôi phân tích danh sách "CẦN LÀM" hiện tại và gợi ý độ ưu tiên không?
              </div>
            </div>
          </div>

          <div class="ai-input-area">
            <div class="ai-input-wrapper">
              <input type="text" placeholder="Hỏi AI bất cứ điều gì..." />
              <div class="ai-input-actions">
                <i class="fa-solid fa-paperclip"></i>
                <i class="fa-solid fa-face-smile"></i>
                <button class="send-btn"><i class="fa-solid fa-paper-plane"></i></button>
              </div>
            </div>
          </div>
        </aside>
      </transition>
      <input type="file" ref="fileInput" style="display: none" @change="handleFileUpload" />

      <!-- Create Task Modal -->
      <el-dialog
        v-model="showCreateModal"
        title="Tạo công việc mới"
        width="600px"
        custom-class="create-task-dialog"
      >
        <div class="create-task-form">
          <div class="form-item">
            <label>Tiêu đề *</label>
            <el-input v-model="newTask.title" placeholder="Tên công việc..." />
          </div>
          
          <div class="form-item">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px;">
              <label style="margin-bottom: 0;">Mô tả</label>
              <el-button size="small" type="primary" plain @click="breakdownTaskAI" :loading="isAIBreakingDown" style="border-radius: 20px; font-size: 11px; padding: 4px 10px;">
                <i class="fa-solid fa-wand-magic-sparkles" style="margin-right: 4px;"></i> Phân tách bằng AI
              </el-button>
            </div>
            <el-input 
              v-model="newTask.description" 
              type="textarea" 
              :rows="4" 
              placeholder="Thêm mô tả chi tiết..." 
            />
          </div>

          <!-- AI Subtasks Preview -->
          <div v-if="aiSubtasks.length > 0" class="ai-subtasks-preview" style="margin-bottom: 16px; padding: 12px; background: rgba(59, 130, 246, 0.05); border-left: 3px solid #3b82f6; border-radius: 4px;">
            <div style="font-size: 13px; font-weight: 600; color: #3b82f6; margin-bottom: 8px;">
              <i class="fa-solid fa-list-check"></i> Các công việc phụ đề xuất (AI)
            </div>
            <div v-for="(sub, idx) in aiSubtasks" :key="idx" style="display: flex; align-items: center; gap: 8px; font-size: 12px; margin-bottom: 6px; color: var(--text-primary);">
              <i class="fa-regular fa-square"></i>
              <span style="flex: 1;">{{ sub.title }}</span>
              <span style="color: #64748b; font-size: 11px;">{{ sub.estHours }}h</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-item half">
              <label>Trạng thái</label>
              <el-select v-model="newTask.statusName" placeholder="Chọn trạng thái">
                <el-option label="TO DO" value="TO DO" />
                <el-option label="IN PROGRESS" value="IN PROGRESS" />
                <el-option label="DONE" value="DONE" />
              </el-select>
            </div>
            <div class="form-item half">
              <label>Độ ưu tiên</label>
              <el-select v-model="newTask.priority" placeholder="Chọn độ ưu tiên">
                <el-option :label="1" :value="1">1 - Khẩn cấp</el-option>
                <el-option :label="2" :value="2">2 - Cao</el-option>
                <el-option :label="3" :value="3">3 - Trung bình</el-option>
                <el-option :label="4" :value="4">4 - Thấp</el-option>
              </el-select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-item half">
              <label>Người thực hiện</label>
              <el-select 
                v-model="newTask.assignedUserId" 
                :placeholder="isValidProject ? 'Phân công cho...' : 'Dự án không hợp lệ'" 
                :loading="isFetchingMembers"
                clearable
              >
                <el-option 
                  v-for="member in projectMembers" 
                  :key="member.userId" 
                  :label="member.fullName" 
                  :value="member.userId"
                >
                  <div style="display: flex; align-items: center; gap: 8px;">
                    <div class="avatar-tiny">{{ member.fullName ? member.fullName[0] : 'U' }}</div>
                    <span>{{ member.fullName }}</span>
                  </div>
                </el-option>
              </el-select>
            </div>
            <div class="form-item half">
              <label>Ngày hết hạn</label>
              <el-date-picker
                v-model="newTask.dueDate"
                type="date"
                placeholder="Chọn ngày"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </div>
          </div>
        </div>
        <template #footer>
          <div class="dialog-footer">
            <el-button @click="showCreateModal = false">Hủy</el-button>
            <el-button type="primary" @click="submitCreateTask" :disabled="!newTask.title">Tạo công việc</el-button>
          </div>
        </template>
      </el-dialog>

    <!-- Start Sprint Dialog -->
    <el-dialog
      v-model="showStartSprintModal"
      title="Start Sprint"
      width="500px"
      custom-class="sprint-dialog"
    >
      <div class="sprint-dialog-content">
         <p class="sprint-subtext"><b>{{ totalTasksCount }}</b> work items will be included in this sprint.</p>
         <p class="sprint-req-text">Required fields are marked with an asterisk <span class="req">*</span></p>
         
         <div class="sprint-form-group">
            <label>Sprint name <span class="req">*</span></label>
            <el-input v-model="sprintData.name" />
         </div>

         <div class="sprint-form-group">
            <label>Duration <span class="req">*</span></label>
            <el-select v-model="sprintData.duration" style="width: 100%;">
               <el-option label="custom" value="custom" />
               <el-option label="1 week" value="1w" />
               <el-option label="2 weeks" value="2w" />
               <el-option label="4 weeks" value="4w" />
            </el-select>
         </div>

         <div class="sprint-date-row" style="display: flex; gap: 16px;">
           <div class="sprint-form-group" style="flex: 1;">
              <label>Start date <span class="req">*</span></label>
              <el-date-picker v-model="sprintData.startDate" type="datetime" format="D/M/YYYY h:mm a" style="width: 100%;" />
           </div>

           <div class="sprint-form-group" style="flex: 1;">
              <label>End date <span class="req">*</span></label>
              <el-date-picker v-model="sprintData.endDate" type="datetime" format="D/M/YYYY h:mm a" style="width: 100%;" />
           </div>
         </div>

         <div class="sprint-form-group">
            <label>Sprint goal</label>
            <el-input type="textarea" :rows="4" v-model="sprintData.goal" />
         </div>
      </div>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showStartSprintModal = false" text style="color: #42526e; font-weight: 500;">Cancel</el-button>
          <el-button type="primary" @click="startSprint" style="background-color: #0052cc;">Start</el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Complete Sprint Dialog -->
    <el-dialog
      v-model="showCompleteSprintModal"
      width="500px"
      custom-class="sprint-dialog complete-sprint-dialog"
      :show-close="false"
    >
      <div class="complete-sprint-content">
         <div class="complete-header-img" style="text-align: center; margin-bottom: 20px;">
            <svg width="120" height="90" viewBox="0 0 120 90" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M43.5 75L53.5 45L63.5 75L73.5 60L83.5 75V35H43.5V75Z" fill="#1D4ED8"/>
              <path d="M48.5 83L60 50L71.5 83L83 62L94.5 83V42H48.5V83Z" fill="#2563EB"/>
              <circle cx="60" cy="35" r="24" fill="#FBBF24"/>
              <circle cx="60" cy="35" r="18" fill="#F59E0B"/>
              <path d="M60 21L63.5 28L71 29L65.5 34.5L67 42L60 38.5L53 42L54.5 34.5L49 29L56.5 28L60 21Z" fill="#2563EB"/>
            </svg>
         </div>
         <h2 class="complete-title" style="text-align: center; font-size: 20px; font-weight: 500; margin-bottom: 16px;">Complete {{ sprintData.name }}</h2>
         
         <p class="complete-desc" style="font-size: 14px; text-align: center; margin-bottom: 24px;">
            This sprint contains <b>{{ completedTasksCount }} completed work items</b> and <b>{{ totalTasksCount - completedTasksCount }} open work items</b>.
         </p>
         
         <ul class="complete-list" style="font-size: 14px; color: #42526e; margin-bottom: 24px; padding-left: 20px;">
            <li style="margin-bottom: 8px;">Completed work items includes everything in the last column on the board, <a href="#" style="color: #0052cc; text-decoration: none;">Done</a>.</li>
            <li>Open work items includes everything from any other column on the board. Move these to a new sprint or the backlog.</li>
         </ul>

         <div class="sprint-form-group">
            <label style="margin-bottom: 8px; display: block; font-weight: 600; font-size: 12px; color: #42526e;">Move open work items to</label>
            <el-select v-model="moveOpenTo" style="width: 100%;">
               <el-option label="New sprint" value="new" />
               <el-option label="Backlog" value="backlog" />
            </el-select>
         </div>

         <div class="retro-box" style="background: #f4f5f7; padding: 16px; border-radius: 4px; margin-top: 24px;">
            <el-checkbox v-model="createRetro" style="font-weight: 600;">Create a retrospective for this sprint</el-checkbox>
            <p class="retro-subtext" style="font-size: 12px; color: #42526e; margin-top: 8px; line-height: 1.5; padding-left: 24px;">Finish off your sprint with a Confluence retrospective! Contribute to your team's culture and improve how you work.</p>
         </div>
      </div>
      <template #footer>
        <span class="dialog-footer" style="display: flex; justify-content: flex-end; width: 100%;">
          <el-button @click="showCompleteSprintModal = false" text style="color: #42526e; font-weight: 500;">Cancel</el-button>
          <el-button type="primary" @click="completeSprint" style="background-color: #0052cc;">Complete sprint</el-button>
        </span>
      </template>
    </el-dialog>

    <!-- Burndown Chart Modal -->
    <el-dialog v-model="showBurndownModal" title="Sprint Burndown Chart" width="800px" custom-class="dark-dialog">
      <div v-if="isFetchingBurndown" style="text-align: center; padding: 40px; color: #94a3b8;">
        <i class="fa-solid fa-circle-notch fa-spin"></i> Đang tải dữ liệu...
      </div>
      <div v-else-if="burndownSeries[0].data.length === 0" style="text-align: center; padding: 40px; color: #94a3b8;">
        Không đủ dữ liệu cho Sprint này.
      </div>
      <div v-else class="burndown-chart-wrapper" style="background: var(--bg-nav); padding: 16px; border-radius: 8px;">
        <apexchart type="line" height="350" :options="burndownOptions" :series="burndownSeries"></apexchart>
      </div>
    </el-dialog>
    
    
    <AddPeopleModal v-model:visible="showAddPeopleModal" @added="handleAddedPeople" />
    <!-- Customize Sidebar Modal -->
    <CustomizeSidebarModal :visible="showCustomizeModal" @update:visible="showCustomizeModal = $event" @saved="handleSidebarSaved" />
  </NexusLayout>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Search } from '@element-plus/icons-vue'
import logoImg from '../assets/logo_QLCV.png'
import HelpDropdown from '../components/HelpDropdown.vue'
import SettingsDropdown from '../components/SettingsDropdown.vue'
import NotificationsDropdown from '../components/NotificationsDropdown.vue'
import UserDropdown from '../components/UserDropdown.vue'
import AddPeopleModal from '../components/AddPeopleModal.vue'
import CustomizeSidebarModal from '../components/CustomizeSidebarModal.vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import KanbanBoard from '@/components/KanbanBoard.vue'
import TaskDetailModal from '@/components/TaskDetailModal.vue'
import axiosClient from '../api/axiosClient'
import draggable from 'vuedraggable'
import * as echarts from 'echarts'
import VueApexCharts from 'vue3-apexcharts'
import { signalRService } from '@/api/signalrService'


const route = useRoute()
const projectId = computed(() => route.params.id)

const showAddPeopleModal = ref(false)
const handleAddedPeople = (data) => {
  console.log('Added people:', data)
  fetchProjectMembers()
}

const searchQuery = ref('')
const aiVisible = ref(false)
const showTeamsDialog = ref(false)
const isStarred = ref(false)
const showAddPeopleDialog = ref(false)
const showSettingsDialog = ref(false)
const addPeopleEmail = ref('')
const addPeopleRole = ref('DEV')
const sidebarVisible = ref(true)

const toggleAI = () => {
  aiVisible.value = !aiVisible.value
}

const currentTab = ref('list')
const activityRange = ref('7d')
const showTaskModal = ref(false)
const selectedTask = ref(null)
const showCreateModal = ref(false)
const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const isAdmin = computed(() => {
  const roles = currentUser.systemRoles || []
  return roles.includes('Admin') || roles.includes('admin')
})

const isPM = computed(() => {
  const roles = currentUser.systemRoles || []
  return roles.includes('Manager') || roles.includes('manager') || roles.includes('PM')
})

const isAuthorizedForAdminActions = computed(() => isAdmin.value || isPM.value)
const newTask = ref({
  title: '',
  description: '',
  statusName: 'TO DO',
  priority: 3,
  assignedUserId: currentUser.id || null,
  dueDate: null
})

const isSprintActive = ref(false)
const showStartSprintModal = ref(false)
const showCompleteSprintModal = ref(false)
const moveOpenTo = ref('new')
const createRetro = ref(true)

const sprintData = ref({
  name: 'SCRUM Sprint 1',
  duration: 'custom',
  startDate: new Date(),
  endDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000), // +2 weeks
  goal: ''
})

const openStartSprintModal = () => {
  showStartSprintModal.value = true
}

const startSprint = () => {
  isSprintActive.value = true
  showStartSprintModal.value = false
  currentTab.value = 'board' // Redirect to board
}

// Burndown Chart logic
const showBurndownModal = ref(false)
const isFetchingBurndown = ref(false)
const burndownSeries = ref([{ name: 'Remaining Task Points', data: [] }, { name: 'Ideal Burn', data: [] }])
const burndownOptions = ref({
  chart: { type: 'line', foreColor: '#94a3b8', toolbar: { show: false } },
  stroke: { curve: 'straight', width: [3, 2], dashArray: [0, 5] },
  colors: ['#ef4444', '#3b82f6'],
  xaxis: { categories: [], tooltip: { enabled: false } },
  yaxis: { min: 0, title: { text: 'Story Points' } },
  markers: { size: 4 },
  theme: { mode: 'dark' },
  grid: { borderColor: '#334155' }
})

const openBurndownChart = async (sprintId) => {
  showBurndownModal.value = true
  isFetchingBurndown.value = true
  try {
    const res = await axiosClient.get(`/projects/${projectId.value}/sprints/${sprintId}/burndown`)
    const data = res.data.data || []
    
    burndownOptions.value = {
      ...burndownOptions.value,
      xaxis: { ...burndownOptions.value.xaxis, categories: data.map(d => d.date) }
    }
    
    burndownSeries.value = [
      { name: 'Thực tế', data: data.map(d => d.remainingPoints) },
      { name: 'Lý tưởng', data: data.map(d => d.idealPoints) }
    ]
  } catch (error) {
    ElNotification({ title: 'Lỗi', message: 'Không thể tải biểu đồ Burndown', type: 'error' })
  } finally {
    isFetchingBurndown.value = false
  }
}

const openCompleteSprintModal = () => {
  showCompleteSprintModal.value = true
}

const completeSprint = () => {
  isSprintActive.value = false
  showCompleteSprintModal.value = false
  currentTab.value = 'board' // Stay on board, which returns to empty state
  
  // Optional mockup logic: increment sprint run number for next time
  const match = sprintData.value.name.match(/\d+$/);
  if (match) {
    const num = parseInt(match[0], 10);
    sprintData.value.name = sprintData.value.name.replace(/\d+$/, (num + 1).toString());
  }
}

const formatDateShort = (dateString) => {
  if (!dateString) return '';
  const parts = dateString.split('-');
  if (parts.length >= 3) {
    const d = new Date(dateString);
    return d.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' });
  }
  return dateString;
}

const getStatusStoryPoints = (group, statusKey) => {
  if (!group || !group.items) return 0;
  const tasksInStatus = group.items.filter(t => (t.statusName || '').toUpperCase().replace(/\s/g, '') === statusKey.replace(/\s/g, ''));
  return tasksInStatus.reduce((acc, t) => acc + (t.storyPointEstimate || t.storyPoints || Math.floor(Math.random() * 3) + 1), 0);
}

const handleSprintMenu = (command) => {
  if (command === 'reorder') ElMessage.info('Reorder work items feature not implemented');
  else if (command === 'edit') ElMessage.info('Edit sprint feature not implemented');
  else if (command === 'delete') ElMessage.info('Delete sprint feature not implemented');
}

const boardTaskGroups = computed(() => {
  if (!isSprintActive.value) {
    // Show empty groups when no sprint is active
    return taskGroups.value.map(g => ({ ...g, items: [] }))
  }
  return taskGroups.value
})

const totalTasksCount = computed(() => {
  return taskGroups.value.reduce((sum, g) => sum + g.items.length, 0)
})

const completedTasksCount = computed(() => {
  const doneGroup = taskGroups.value.find(g => g.statusText === 'DONE')
  return doneGroup ? doneGroup.items.length : 0
})
const projectMembers = ref([])
const isFetchingMembers = ref(false)
const isValidProject = ref(true)

const canManageMembers = computed(() => {
  if (!currentUser || !currentUser.id) return false;
  if (currentUser.role && typeof currentUser.role === 'string' && currentUser.role.includes('Admin')) return true;
  if (!projectMembers.value || !Array.isArray(projectMembers.value)) return false;

  const myMemberInfo = projectMembers.value.find(m => m && m.userId === currentUser.id);
  if (!myMemberInfo) return false;
  
  const role = myMemberInfo.projectRole || myMemberInfo.role;
  return role === 'PM' || role === 'PO' || role === 'Admin';
});

const changeMemberRole = async (userId, newRole) => {
  try {
    const payload = { role: newRole };
    await axiosClient.put(`/projects/${projectId.value}/members/${userId}/role`, payload);
    ElMessage.success('Cập nhật quyền thành công');
    await fetchProjectMembers();
  } catch (error) {
    console.error('Role update error:', error);
    ElMessage.error(error.response?.data?.message || 'Không thể cập nhật quyền');
  }
};

// (B) Xóa thành viên khỏi dự án (Soft Delete)
const removeMember = async (userId, fullName) => {
  try {
    await ElMessageBox.confirm(
      `Bạn có chắc chắn muốn xóa "${fullName}" khỏi dự án? Các task được giao cho người này sẽ bị gỡ phân công.`,
      'Xóa thành viên',
      { confirmButtonText: 'Xóa', cancelButtonText: 'Hủy', type: 'warning', confirmButtonClass: 'el-button--danger' }
    );
    await axiosClient.delete(`/projects/${projectId.value}/members/${userId}`);
    ElMessage.success(`Đã xóa ${fullName} khỏi dự án`);
    await fetchProjectMembers();
  } catch (error) {
    if (error !== 'cancel') {
        console.error('Remove member error:', error);
        ElMessage.error(error.response?.data?.message || 'Không thể xóa thành viên');
    }
  }
};

const inviteMember = async () => {
  if (!addPeopleEmail.value) {
    ElMessage.warning('Vui lòng nhập email thành viên');
    return;
  }
  try {
    await axiosClient.post(`/projects/${projectId.value}/members`, {
      email: addPeopleEmail.value,
      role: addPeopleRole.value
    });
    ElMessage.success(`Đã mời ${addPeopleEmail.value} với vai trò ${addPeopleRole.value}`);
    addPeopleEmail.value = '';
    addPeopleRole.value = 'DEV';
    await fetchProjectMembers();
  } catch (error) {
    console.error('Invite member error:', error);
    ElMessage.error(error.response?.data?.message || 'Không thể mời thành viên. Kiểm tra lại email.');
  }
};

// Filtering & Sorting State
const activeFilters = ref({
  assigneeName: null,
  priority: null
})
const sortBy = ref(null) 
const showCompleted = ref(true)
const groupBy = ref('status') 
const groupByOrder = ref('asc') 
const showEmptyGroups = ref(true)

const sidebarPreferences = ref({
  audit: true,
  users: true
})

const filteredTasks = computed(() => {
  const q = (searchQuery.value || '').toLowerCase();
  const isMgr = isAdmin.value || isPM.value;
  const currentUid = currentUser.id;
  const filterByAssignee = activeFilters.value.assigneeName;
  const filterByPrio = activeFilters.value.priority;
  const showD = showCompleted.value;

  let result = tasks.value.filter(t => {
    if (!isMgr && t.assignedUserId !== currentUid) return false;
    if (q && !(t.title.toLowerCase().includes(q) || (t.reporterName && t.reporterName.toLowerCase().includes(q)))) return false;
    if (!showD && t.statusName === 'DONE') return false;
    if (filterByAssignee && t.assigneeName !== filterByAssignee) return false;
    if (filterByPrio && t.priority !== filterByPrio) return false;
    return true;
  });

  if (sortBy.value === 'title') {
    result.sort((a, b) => a.title.localeCompare(b.title));
  } else if (sortBy.value === 'date') {
    result.sort((a, b) => new Date(a.plannedEndDate || 0) - new Date(b.plannedEndDate || 0));
  } else if (sortBy.value === 'priority') {
    result.sort((a, b) => (b.priority || 0) - (a.priority || 0));
  }

  return result;
});

// Tasks state
const tasks = ref([])
const comments = ref([])
const showCustomizeModal = ref(false)


const backlogTaskGroups = computed(() => {
  const allTasks = filteredTasks.value.filter(t => t.sprintId == null && t.statusName !== 'DONE');
  const gb = groupBy.value;
  const gOrder = groupByOrder.value;

  if (gb === 'status') {
    const map = { TODO: [], INPROGRESS: [], INREVIEW: [], DONE: [] };
    allTasks.forEach(t => {
      const s = (t.statusName || 'TO DO').toUpperCase().replace(/s/g, '');
      if (map[s]) map[s].push(t);
      else map.TODO.push(t);
    });

    let groups = [
      { id: 'bgrp-todo', statusText: 'TO DO', statusBg: '#374151', statusColor: '#9ca3af', expanded: true, items: map.TODO },
      { id: 'bgrp-progress', statusText: 'IN PROGRESS', statusBg: '#6b21a8', statusColor: '#ffffff', expanded: true, items: map.INPROGRESS },
      { id: 'bgrp-review', statusText: 'IN REVIEW', statusBg: '#0369a1', statusColor: '#ffffff', expanded: true, items: map.INREVIEW }
    ];
    if (gOrder === 'desc') groups.reverse();
    return groups;
  }
  return [];
});

const taskGroups = computed(() => {
  const allTasks = filteredTasks.value;
  const gb = groupBy.value;
  const gOrder = groupByOrder.value;

  if (gb === 'status') {
    const map = { TODO: [], INPROGRESS: [], INREVIEW: [], DONE: [] };
    allTasks.forEach(t => {
       const s = (t.statusName || '').toUpperCase().replace(/\s/g, '');
       if (map[s]) map[s].push(t);
       else if (s === 'TODO' || (s !== 'INPROGRESS' && s !== 'DONE' && s !== 'INREVIEW')) map.TODO.push(t);
    });

    let groups = [
      { id: 'grp-todo', statusText: 'TO DO', statusBg: '#374151', statusColor: '#9ca3af', expanded: true, items: map.TODO, showQuickAdd: false, quickAddTitle: '' },
      { id: 'grp-progress', statusText: 'IN PROGRESS', statusBg: '#6b21a8', statusColor: '#ffffff', expanded: true, items: map.INPROGRESS, showQuickAdd: false, quickAddTitle: '' },
      { id: 'grp-review', statusText: 'IN REVIEW', statusBg: '#0369a1', statusColor: '#ffffff', expanded: true, items: map.INREVIEW, showQuickAdd: false, quickAddTitle: '' },
      { id: 'grp-done', statusText: 'DONE', statusBg: '#166534', statusColor: '#ffffff', expanded: true, items: map.DONE, showQuickAdd: false, quickAddTitle: '' }
    ];
    if (gOrder === 'desc') groups.reverse();
    return groups;

  } else if (gb === 'priority') {
    const pmap = { 1: [], 2: [], 3: [], 4: [] };
    allTasks.forEach(t => { if (pmap[t.priority]) pmap[t.priority].push(t); });
    const priorities = [
      { val: 1, text: 'URGENT', bg: '#ef4444' },
      { val: 2, text: 'HIGH', bg: '#f97316' },
      { val: 3, text: 'NORMAL', bg: '#3b82f6' },
      { val: 4, text: 'LOW', bg: '#94a3b8' }
    ];
    let groups = priorities.map(p => ({
      id: `grp-prio-${p.val}`, statusText: p.text, statusBg: p.bg, statusColor: '#ffffff', expanded: true, items: pmap[p.val], showQuickAdd: false, quickAddTitle: '', priorityValue: p.val
    }));
    if (gOrder === 'desc') groups.reverse();
    return groups;

  } else if (gb === 'assignee') {
     const amap = {};
     projectMembers.value.forEach(m => { if(m && m.userId) amap[m.userId] = []; });
     const unassigned = [];
     allTasks.forEach(t => {
        if (t.assignedUserId && amap[t.assignedUserId]) amap[t.assignedUserId].push(t);
        else unassigned.push(t);
     });
     const groups = projectMembers.value.map(m => ({
       id: `grp-member-${m.userId}`, statusText: m.fullName, statusBg: '#475569', statusColor: '#ffffff', expanded: true, items: amap[m.userId], showQuickAdd: false, quickAddTitle: ''
     }));
     groups.push({ id: 'grp-unassigned', statusText: 'Chưa phân công', statusBg: '#1e293b', statusColor: '#94a3b8', expanded: true, items: unassigned, showQuickAdd: false, quickAddTitle: '' });
     if (gOrder === 'desc') groups.reverse();
     return groups;
  } else {
    return [{ id: 'grp-default', statusText: 'Tất cả', statusBg: '#334155', statusColor: '#ffffff', expanded: true, items: allTasks, showQuickAdd: false, quickAddTitle: '' }];
  }
});


// Charts instances
let statusChart = null
let priorityChart = null
let activityChart = null

const initCharts = () => {
  const isDark = document.documentElement.classList.contains('dark')
  const textColor = isDark ? '#f1f5f9' : '#1e293b'
  const splitLineColor = isDark ? '#334155' : '#e2e8f0'

  const statusDom = document.getElementById('status-donut-chart')
  if (statusDom) {
    if (statusChart) statusChart.dispose()
    statusChart = echarts.init(statusDom)
    updateStatusChart(textColor)
  }

  const priorityDom = document.getElementById('priority-bar-chart')
  if (priorityDom) {
    if (priorityChart) priorityChart.dispose()
    priorityChart = echarts.init(priorityDom)
    updatePriorityChart(textColor, splitLineColor)
  }

  const activityDom = document.getElementById('activity-line-chart')
  if (activityDom) {
    if (activityChart) activityChart.dispose()
    activityChart = echarts.init(activityDom)
    updateActivityChart(textColor, splitLineColor)
  }
}

const handleStatusHover = (statusKey) => {
  hoveredStatus.value = statusKey
  if (!statusChart) return
  
  let index = 0, name = '', value = 0;
  const counts = statusCounts.value;
  if(statusKey === 'TODO') { index = 0; name = 'Cần làm'; value = counts['TODO'] || 0; }
  else if(statusKey === 'INPROGRESS') { index = 1; name = 'Đang thực hiện'; value = counts['IN PROGRESS'] || 0; }
  else if(statusKey === 'DONE') { index = 2; name = 'Đã hoàn thành'; value = counts['DONE'] || 0; }
  
  statusChart.dispatchAction({ type: 'highlight', seriesIndex: 0, dataIndex: index });
  
  const total = (counts['TODO'] || 0) + (counts['IN PROGRESS'] || 0) + (counts['DONE'] || 0)
  const percent = total > 0 ? Math.round((value / total) * 100) : 0
  
  statusChart.setOption({ title: { text: `${percent}%\n{sub|${name}}` } })
}

const handleStatusLeave = () => {
  hoveredStatus.value = null
  if (!statusChart) return
  statusChart.dispatchAction({ type: 'downplay', seriesIndex: 0 });
  const counts = statusCounts.value
  const total = (counts['TODO'] || 0) + (counts['IN PROGRESS'] || 0) + (counts['DONE'] || 0)
  statusChart.setOption({ title: { text: `${total}\n{sub|Công việc}` } })
}

const updateStatusChart = (textColor) => {
  if (!statusChart) return
  const counts = statusCounts.value
  const total = (counts['TODO'] || 0) + (counts['IN PROGRESS'] || 0) + (counts['DONE'] || 0)

  statusChart.setOption({
    tooltip: { trigger: 'item' },
    legend: { show: false },
    title: {
      text: `${total}\n{sub|Công việc}`,
      left: 'center',
      top: 'center',
      textStyle: {
        fontSize: 24,
        fontWeight: 'bold',
        color: textColor,
        rich: {
          sub: {
            fontSize: 14,
            color: '#64748b',
            align: 'center',
            padding: [4, 0, 0, 0]
          }
        }
      }
    },
    series: [{
      type: 'pie',
      radius: ['65%', '85%'],
      avoidLabelOverlap: false,
      itemStyle: { borderColor: 'transparent', borderWidth: 2 },
      label: { show: false },
      data: [
        { value: counts['TODO'] || 0, name: 'Cần làm', itemStyle: { color: '#c4b5fd' } },
        { value: counts['IN PROGRESS'] || 0, name: 'Đang thực hiện', itemStyle: { color: '#93c5fd' } },
        { value: counts['DONE'] || 0, name: 'Đã hoàn thành', itemStyle: { color: '#86efac' } }
      ]
    }]
  })

  // Hook hover logic so that hovering natively triggers the same percent title!
  if (!statusChart._hoverBound) {
    statusChart.on('mouseover', { seriesType: 'pie' }, (params) => {
       const counts = statusCounts.value
       const total = (counts['TODO'] || 0) + (counts['IN PROGRESS'] || 0) + (counts['DONE'] || 0)
       const percent = total > 0 ? Math.round((params.value / total) * 100) : 0
       statusChart.setOption({ title: { text: `${percent}%\n{sub|${params.name}}` } })
       
       if (params.name === 'Cần làm') hoveredStatus.value = 'TODO';
       else if (params.name === 'Đang thực hiện') hoveredStatus.value = 'INPROGRESS';
       else if (params.name === 'Đã hoàn thành') hoveredStatus.value = 'DONE';
    })
    statusChart.on('mouseout', { seriesType: 'pie' }, () => {
       const counts = statusCounts.value
       const total = (counts['TODO'] || 0) + (counts['IN PROGRESS'] || 0) + (counts['DONE'] || 0)
       statusChart.setOption({ title: { text: `${total}\n{sub|Công việc}` } })
       hoveredStatus.value = null;
    })
    statusChart._hoverBound = true
  }
}

const updatePriorityChart = (textColor, splitLineColor) => {
  if (!priorityChart) return
  const pCounts = [
    tasks.value.filter(t => t.priority === 1).length,
    tasks.value.filter(t => t.priority === 2).length,
    tasks.value.filter(t => t.priority === 3).length,
    tasks.value.filter(t => t.priority === 4).length
  ]

  priorityChart.setOption({
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { left: '3%', right: '4%', bottom: '15%', top: '5%', containLabel: true },
    xAxis: { 
      type: 'category', 
      data: ['Urgent', 'High', 'Normal', 'Low'],
      axisLabel: { color: textColor },
      axisLine: { lineStyle: { color: splitLineColor } }
    },
    yAxis: { 
      type: 'value',
      axisLabel: { color: textColor },
      splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } }
    },
    series: [{
      data: pCounts,
      type: 'bar',
      barWidth: '40%',
      itemStyle: {
        borderRadius: [4, 4, 0, 0],
        color: (params) => {
          const colors = ['#ef4444', '#f97316', '#3b82f6', '#94a3b8']
          return colors[params.dataIndex]
        }
      }
    }]
  })
}



// Combined chart watch - removed deep: true for performance
watch(tasks, () => {
  const isDark = document.documentElement.classList.contains('dark')
  const textColor = isDark ? '#f1f5f9' : '#1e293b'
  const splitLineColor = isDark ? '#334155' : '#e2e8f0'
  updateStatusChart(textColor)
  updatePriorityChart(textColor, splitLineColor)
  updateActivityChart(textColor, splitLineColor)
})

const updateActivityChart = (textColor, splitLineColor) => {
  if (!activityChart) return

  const last7Days = []
  for (let i = 6; i >= 0; i--) {
    const d = new Date()
    d.setDate(d.getDate() - i)
    last7Days.push(d)
  }

  const dayLabels = last7Days.map(d => d.toLocaleDateString('en-US', { weekday: 'short' }))
  
  // Metric A: Created Tasks
  const createdData = last7Days.map(d => {
    const ds = d.toISOString().split('T')[0]
    return tasks.value.filter(t => (t.createdAt || '').startsWith(ds)).length
  })

  // Metric B: Completed/Updated Tasks
  const updatedData = last7Days.map(d => {
    const ds = d.toISOString().split('T')[0]
    return tasks.value.filter(t => (t.updatedAt || t.createdAt || '').startsWith(ds)).length
  })

  activityChart.setOption({
    tooltip: { trigger: 'axis' },
    legend: {
      data: ['Metric A', 'Metric B'],
      bottom: 0,
      textStyle: { color: textColor }
    },
    grid: { left: '3%', right: '4%', bottom: '15%', top: '10%', containLabel: true },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: dayLabels,
      axisLabel: { color: textColor },
      axisLine: { lineStyle: { color: splitLineColor } }
    },
    yAxis: {
      type: 'value',
      axisLabel: { color: textColor },
      splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } }
    },
    series: [
      {
        name: 'Metric A',
        type: 'line',
        smooth: true,
        data: createdData,
        itemStyle: { color: '#3b82f6' },
        lineStyle: { width: 3 },
        symbol: 'circle',
        symbolSize: 8
      },
      {
        name: 'Metric B',
        type: 'line',
        smooth: true,
        data: updatedData,
        itemStyle: { color: '#a855f7' },
        lineStyle: { width: 3 },
        symbol: 'circle',
        symbolSize: 8
      }
    ]
  })
}

const weekDays = computed(() => {
  const days = []
  const now = new Date()
  const startOfWeek = new Date(now)
  startOfWeek.setDate(now.getDate() - now.getDay())

  for (let i = 0; i < 7; i++) {
    const d = new Date(startOfWeek)
    d.setDate(startOfWeek.getDate() + i)
    days.push({
      name: d.toLocaleDateString('en-US', { weekday: 'short' }),
      day: d.getDate(),
      date: d.toISOString().split('T')[0],
      isToday: d.toDateString() === now.toDateString()
    })
  }
  return days
})

watch(currentTab, (newTab) => {
  if (newTab === 'summary') {
    setTimeout(initCharts, 200)
  }
})

const openTaskDetail = (task) => {
  selectedTask.value = task
  showTaskModal.value = true
  fetchComments(task.id)
}

const router = useRouter()
const goToDashboard = () => {
  router.push('/dashboard')
}

const goToAI = () => {
  router.push('/ai-assistant')
}

// Fetch tasks
const isValidGuid = (val) => /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(val)

const fetchTasks = async () => {
  if (!projectId.value || !isValidGuid(projectId.value)) {
    console.warn('Invalid projectId:', projectId.value)
    return
  }
  try {
    const { data } = await axiosClient.get(`/projects/${projectId.value}/WorkTasks`)
    let rawTasks = data.data || data || []

    const taskMap = {}
    rawTasks.forEach(t => {
      t.childTasks = []
      t.showChildren = true
      t.newSubtaskTitle = ''
      taskMap[t.id] = t
    })
    
    const roots = []
    rawTasks.forEach(t => {
      if (t.parentTaskId && taskMap[t.parentTaskId]) {
        taskMap[t.parentTaskId].childTasks.push(t)
      } else {
        roots.push(t)
      }
    })
    
    tasks.value = roots
  } catch (error) {
    if (error.response && error.response.status === 403) {
      ElMessage.error(error.response.data?.message || 'Bạn không có quyền truy cập dự án này.')
      router.push('/dashboard')
    } else {
      console.error('Fetch tasks error:', error)
      ElMessage.error('Không thể tải danh sách công việc')
    }
  }
}

const createSubtask = async (parentTask) => {
    if (!parentTask.newSubtaskTitle?.trim()) return;
    try {
        const payload = {
            title: parentTask.newSubtaskTitle,
            projectId: projectId.value,
            statusName: 'TO DO',
            priority: 3,
            assignedUserId: currentUser.id || null,
            parentTaskId: parentTask.id
        }
        await axiosClient.post(`/projects/${projectId.value}/WorkTasks`, payload)
        parentTask.newSubtaskTitle = ''
        await fetchTasks()
        ElMessage.success('Tạo Subtask thành công!')
    } catch (e) {
        ElMessage.error('Không thể tạo Subtask')
        console.error(e)
    }
}

const fetchComments = async (taskId) => {
  try {
    const { data } = await axiosClient.get(`/projects/${projectId.value}/WorkTasks/${taskId}/comments`)
    comments.value = data.data || []
  } catch (error) {
    console.error('Fetch comments error:', error)
  }
}

const seedTestTasks = async () => {
  if (!projectId.value || !isValidGuid(projectId.value)) {
    ElMessage.error('ID dự án không hợp lệ.')
    return
  }
  try {
    for (let i = 1; i <= 5; i++) {
        await axiosClient.post(`/projects/${projectId.value}/WorkTasks`, {
            title: `Công việc TEST ${i}`,
            description: `Dữ liệu mẫu để kiểm tra tính năng kéo thả số ${i}`,
            statusName: 'TO DO',
            priority: Math.floor(Math.random() * 4) + 1,
            assignedUserId: currentUser.id || null,
            projectId: projectId.value
        })
    }
    await fetchTasks()
    ElMessage.success('Đã tạo 5 công việc mẫu thành công!')
  } catch (error) {
    console.error('Seed tasks error:', error)
    ElMessage.error('Không thể tạo công việc mẫu. Vui lòng kiểm tra lại dự án.')
  }
}

const hoveredStatus = ref(null)

// Computed properties for Top Widgets
// Optimized single-pass statistics
const aggregatedStats = computed(() => {
  const now = new Date();
  const sevenDaysAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
  const sevenDaysFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
  sevenDaysFromNow.setHours(23, 59, 59, 999);
  
  const stats = {
    completed7d: 0,
    updated7d: 0,
    created7d: 0,
    dueSoon7d: 0,
    counts: { TODO: 0, 'IN PROGRESS': 0, 'IN REVIEW': 0, DONE: 0 },
    types: { task: 0, epic: 0, story: 0, subtask: 0 }
  };

  tasks.value.forEach(t => {
    const s = (t.statusName || '').toUpperCase().replace(/\s/g, '');
    const cdt = new Date(t.createdAt);
    const udt = t.updatedAt ? new Date(t.updatedAt) : null;
    const ddt = t.dueDate || t.plannedEndDate ? new Date(t.dueDate || t.plannedEndDate) : null;
    
    // Status counts
    if (s === 'DONE') stats.counts.DONE++;
    else if (s === 'INPROGRESS') stats.counts['IN PROGRESS']++;
    else if (s === 'INREVIEW') stats.counts['IN REVIEW']++;
    else stats.counts.TODO++;

    // Date filters (Last 7 days)
    if (s === 'DONE' && (udt || cdt) >= sevenDaysAgo) stats.completed7d++;
    if (udt && udt >= sevenDaysAgo) stats.updated7d++;
    if (cdt >= sevenDaysAgo) stats.created7d++;
    
    // Due soon
    if (s !== 'DONE' && ddt && ddt >= now && ddt <= sevenDaysFromNow) stats.dueSoon7d++;

    // Types
    const type = (t.typeName || 'Task').toLowerCase().replace(/\s/g, '');
    if (stats.types[type] !== undefined) stats.types[type]++;
    else if (type === 'nhiệmvụ' || type === 'task') stats.types.task++;
  });

  return stats;
});

const completedTasksLast7Days = computed(() => aggregatedStats.value.completed7d);
const updatedTasksLast7Days = computed(() => aggregatedStats.value.updated7d);
const createdTasksLast7Days = computed(() => aggregatedStats.value.created7d);
const dueSoonTasksNext7Days = computed(() => aggregatedStats.value.dueSoon7d);
const statusCounts = computed(() => aggregatedStats.value.counts);

const taskTypesBreakdown = computed(() => {
  const total = tasks.value.length || 1;
  const t = aggregatedStats.value.types;
  return [
    { label: 'Nhiệm vụ', icon: 'fa-regular fa-square-check', color: '#3b82f6', percent: Math.round((t.task / total) * 100) },
    { label: 'Sử thi', icon: 'fa-solid fa-bolt', color: '#a855f7', percent: Math.round((t.epic / total) * 100) },
    { label: 'Câu chuyện', icon: 'fa-regular fa-bookmark', color: '#84cc16', percent: Math.round((t.story / total) * 100) },
    { label: 'Nhiệm vụ phụ', icon: 'fa-solid fa-diagram-project', color: '#3b82f6', percent: Math.round((t.subtask / total) * 100) }
  ];
});

const fetchProjectMembers = async () => {
  if (!projectId.value || !isValidGuid(projectId.value)) {
    isValidProject.value = false
    projectMembers.value = []
    return
  }

  isFetchingMembers.value = true
  try {
    const { data } = await axiosClient.get(`/projects/${projectId.value}/members`)
    projectMembers.value = data.data
    isValidProject.value = true

    // RBAC: Check if user has permission to enter this project
    if (!isAdmin.value && !isPM.value) {
       const isMember = projectMembers.value.some(m => m.userId === currentUser.id)
       if (!isMember) {
         ElMessage.error('Bạn không có quyền truy cập vào dự án này (Chưa được add).')
         router.push('/dashboard')
       }
    }
  } catch (error) {
    console.error('Fetch members error:', error)
    isValidProject.value = false
    projectMembers.value = []
    if (error.response?.status === 400 || error.response?.status === 404) {
        ElMessage.error('Không tìm thấy dự án hoặc ID không hợp lệ')
    }
  } finally {
    isFetchingMembers.value = false
  }
}

// AI Task Breakdown feature
const isAIBreakingDown = ref(false)
const aiSubtasks = ref([])

const breakdownTaskAI = async () => {
  if (!newTask.value.title) {
    ElNotification({ title: 'Cảnh báo', message: 'Vui lòng nhập Tiêu đề để AI phân tích', type: 'warning' })
    return
  }
  isAIBreakingDown.value = true
  try {
    const res = await axiosClient.post('/ai/breakdown-task', {
      title: newTask.value.title,
      description: newTask.value.description
    })
    if (res.data.data) {
      aiSubtasks.value = res.data.data
      ElNotification({ title: 'Hoàn tất', message: 'AI đã đề xuất công việc phụ', type: 'success' })
    }
  } catch (error) {
    ElNotification({ title: 'Lỗi', message: 'Không thể gọi AI Service', type: 'error' })
  } finally {
    isAIBreakingDown.value = false
  }
}

const submitCreateTask = async () => {
  if (!newTask.value.title) {
    ElNotification({ title: 'Cảnh báo', message: 'Vui lòng nhập tiêu đề công việc', type: 'warning' })
    return
  }

  // Validate projectId is a valid GUID before calling API
  if (!projectId.value || !isValidGuid(projectId.value)) {
    ElNotification({ title: 'Lỗi', message: 'ID dự án không hợp lệ. Vui lòng truy cập lại từ trang Dashboard.', type: 'error' })
    return
  }
  
  try {
    const payload = {
      title: newTask.value.title,
      description: newTask.value.description || null,
      statusName: newTask.value.statusName || 'TO DO',
      priority: newTask.value.priority || 3,
      typeName: 'Task',
      dueDate: newTask.value.dueDate || null,
      projectId: projectId.value
    }
    // Only add assignedUserId if it's a valid GUID
    if (newTask.value.assignedUserId && newTask.value.assignedUserId !== 'null') {
      payload.assignedUserId = newTask.value.assignedUserId
    }
    const createRes = await axiosClient.post(`/projects/${projectId.value}/WorkTasks`, payload)
    
    // Create subtasks if AI generated them
    if (aiSubtasks.value.length > 0 && createRes.data.data?.id) {
       for (const sub of aiSubtasks.value) {
         await axiosClient.post(`/projects/${projectId.value}/WorkTasks`, {
           title: sub.title,
           description: "Sinh bởi AI. Dự kiến: " + sub.estHours + " giờ",
           statusName: "TO DO",
           priority: 3,
           typeName: "Subtask",
           parentTaskId: createRes.data.data.id,
           projectId: projectId.value
         })
       }
    }

    showCreateModal.value = false
    ElNotification({ title: 'Thành công', message: aiSubtasks.value.length > 0 ? 'Đã tạo công việc & Subtasks' : 'Đã tạo công việc mới', type: 'success' })
    await fetchTasks()
    
    // Reset form
    newTask.value = { title: '', description: '', statusName: 'TO DO', priority: 3, assignedUserId: currentUser.id || null, dueDate: null }
    aiSubtasks.value = [] // Reset AI subtasks
  } catch (error) {
    console.error('Create task error RESPONSE:', error.response?.data || error)
    const errMsg = error.response?.data?.stack || error.response?.data?.inner || error.response?.data?.message || 'Không thể tạo công việc'
    ElNotification({ title: 'Lỗi Server 500 (Vui lòng copy dòng này cho tôi)', message: errMsg, type: 'error', duration: 10000 })
  }
}

// SignalR event handlers
const handleTaskCreated = (newTask) => {
  tasks.value.push(newTask)
  ElNotification({ title: 'Real-time', message: `Task "${newTask.title}" created`, type: 'success' })
}

const handleTaskUpdated = (updatedTask) => {
  const index = tasks.value.findIndex(t => t.id === updatedTask.id)
  if (index !== -1) {
    tasks.value[index] = updatedTask
  }
}

const handleTaskMoved = (movedTask) => {
  handleTaskUpdated(movedTask)
}

const handleTaskDeleted = (taskId) => {
  tasks.value = tasks.value.filter(t => t.id !== taskId)
}

const handleCommentAdded = (taskId, comment) => {
  if (selectedTask.value && selectedTask.value.id === taskId) {
    comments.value.push(comment)
  }
}

const handleFileUploaded = (taskId, attachment) => {
  if (selectedTask.value && selectedTask.value.id === taskId) {
    ElNotification({ title: 'Tệp mới', message: `Đã tải lên: ${attachment.fileName}`, type: 'info' })
  }
}

// Watch for members dialog opening to fetch members
watch(showTeamsDialog, async (newVal) => {
  if (newVal) {
    await fetchProjectMembers()
  }
})

// Open members dialog helper
const openMembersDialog = () => {
  showTeamsDialog.value = true
}

// Space menu command handler
const handleSpaceMenuCommand = async (command) => {
  switch (command) {
    case 'star':
      isStarred.value = !isStarred.value
      ElMessage.success(isStarred.value ? 'Đã thêm vào mục yêu thích' : 'Đã xóa khỏi mục yêu thích')
      break
    case 'add-people':
      showAddPeopleModal.value = true
      break
    case 'save-template':
      ElMessage.info('Tính năng Save as Template chỉ khả dụng cho gói Enterprise')
      break
    case 'set-background':
      ElMessage.info('Tính năng đặt hình nền đang được phát triển')
      break
    case 'settings':
      ElMessage.info('Tính năng cài đặt không gian đang được phát triển')
      break
    case 'archive':
      try {
        await ElMessageBox.confirm(
          'Bạn có chắc muốn lưu trữ không gian này? Các công việc sẽ bị ẩn khỏi bảng điều khiển.',
          'Lưu trữ không gian',
          { confirmButtonText: 'Lưu trữ', cancelButtonText: 'Hủy', type: 'warning' }
        )
        ElMessage.success('Không gian đã được lưu trữ (Archive)')
        router.push('/dashboard')
      } catch { /* user cancelled */ }
      break
    case 'delete':
      try {
        await ElMessageBox.confirm(
          'Bạn có chắc chắn muốn xóa không gian này? Hành động này không thể hoàn tác!',
          'Xóa không gian',
          { confirmButtonText: 'Xóa', cancelButtonText: 'Hủy', type: 'error', confirmButtonClass: 'el-button--danger' }
        )
        await axiosClient.delete(`/projects/${projectId.value}`)
        ElMessage.success('Đã xóa không gian thành công')
        router.push('/dashboard')
      } catch (error) {
        if (error !== 'cancel') {
          console.error('Delete space error:', error)
          ElMessage.error(error.response?.data?.message || 'Không thể xóa không gian')
        }
      }
      break
  }
}

const handleSidebarSaved = (prefs) => {
  const newPrefs = { ...sidebarPreferences.value }
  if (prefs && prefs.navItems) {
    prefs.navItems.forEach(item => {
      if (['recent', 'spaces', 'ai', 'audit', 'users'].includes(item.id)) {
        newPrefs[item.id] = item.checked
      }
    })
  }
  sidebarPreferences.value = newPrefs
  localStorage.setItem('sidebarPreferences', JSON.stringify(newPrefs))
}

onMounted(async () => {
  const saved = localStorage.getItem('sidebarPreferences')
  if (saved) {
    try {
      Object.assign(sidebarPreferences.value, JSON.parse(saved))
    } catch (e) {}
  }

  await fetchTasks()
  await fetchProjectMembers()
  if (projectId.value) {
    try {
      await signalRService.startConnection(projectId.value)
      signalRService.on('TaskCreated', handleTaskCreated)
      signalRService.on('TaskUpdated', handleTaskUpdated)
      signalRService.on('TaskMoved', handleTaskMoved)
      signalRService.on('TaskDeleted', handleTaskDeleted)
      signalRService.on('CommentAdded', handleCommentAdded)
      signalRService.on('FileUploaded', handleFileUploaded)
    } catch (err) {
      console.warn('SignalR không khả dụng, tính năng real-time bị tạm tắt:', err.message)
    }
  }

  // Handle theme changes for charts
  setTimeout(() => {
    initCharts()
    window.addEventListener('resize', handleResize)
    themeObserver.observe(document.documentElement, { attributes: true })
  }, 300)
})

const handleResize = () => {
  statusChart?.resize()
  priorityChart?.resize()
  typeChart?.resize()
}

const themeObserver = new MutationObserver((mutations) => {
  mutations.forEach((mutation) => {
    if (mutation.attributeName === 'class') {
      initCharts()
    }
  })
})

onUnmounted(() => {
  signalRService.stopConnection()
  window.removeEventListener('resize', handleResize)
  themeObserver.disconnect()
  statusChart?.dispose()
  priorityChart?.dispose()
  typeChart?.dispose()
})

const moveTask = async (taskId, newStatusId, rowVersion, statusName) => {
  try {
    await axiosClient.put(`/projects/${projectId.value}/WorkTasks/${taskId}/status`, {
      taskStatusId: newStatusId || '00000000-0000-0000-0000-000000000000',
      statusName: statusName,
      rowVersion: rowVersion
    })
    // Refresh tasks to get updated rowVersion from server
    await fetchTasks()
  } catch (error) {
    if (error.response?.status === 409) {
      ElNotification({ title: 'Xung đột', message: 'Tác vụ đã bị thay đổi bởi người khác. Đang cập nhật lại...', type: 'warning' })
    } else if (error.response?.status === 400) {
      ElNotification({ title: 'Không hợp lệ', message: error.response.data.message || 'Không thể chuyển trạng thái', type: 'error' })
    } else {
      console.error('Move task error:', error)
      ElNotification({ title: 'Lỗi', message: 'Không thể chuyển trạng thái công việc', type: 'error' })
    }
    // Refresh to revert optimistic update
    await fetchTasks()
    throw error // Re-throw so handleDraggableChange can revert
  }
}

const newComment = ref('')
const replyingToCommentId = ref(null)

const topLevelComments = computed(() => {
  const map = {}
  const list = []
  
  comments.value.forEach(c => {
    const clone = { ...c, childComments: [] }
    map[clone.id] = clone
    list.push(clone)
  })

  const roots = []
  list.forEach(c => {
    if (c.parentCommentId && map[c.parentCommentId]) {
      map[c.parentCommentId].childComments.push(c)
    } else {
      roots.push(c)
    }
  })
  
  return roots
})

const startReply = (comment) => {
  if (replyingToCommentId.value !== comment.id) {
    newComment.value = ''
  }
  replyingToCommentId.value = comment.id
  setTimeout(() => {
    const ta = document.getElementById('reply-textarea-' + comment.id)
    if (ta) ta.focus()
  }, 100)
}

const cancelReply = () => {
  replyingToCommentId.value = null
  newComment.value = ''
}

const submitComment = async () => {
  if (!newComment.value || !selectedTask.value) return
  
  try {
    const payload = {
      workTaskId: selectedTask.value.id,
      content: newComment.value
    }
    if (replyingToCommentId.value) {
      payload.parentCommentId = replyingToCommentId.value
    }
    
    const { data } = await axiosClient.post(`/projects/${projectId.value}/Comments`, payload)
    newComment.value = ''
    replyingToCommentId.value = null
    comments.value.push(data.data)
  } catch (error) {
    console.error('Submit comment error:', error)
  }
}

const handleFileUpload = async (event) => {
  const file = event.target.files[0]
  if (!file || !selectedTask.value) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const { data } = await axiosClient.post(`/projects/${projectId.value}/Attachments/upload/${selectedTask.value.id}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    ElNotification({ title: 'Success', message: `File ${data.fileName} uploaded`, type: 'success' })
  } catch (error) {
    console.error('File upload error:', error)
  }
}
const fileInput = ref(null)
const triggerFileUpload = () => fileInput.value?.click()
const currentProjectRole = ref('ADMIN'); 
const canEditBoard = computed(() => !['Guest', 'Stakeholder'].includes(currentProjectRole.value));
const hasRole = (roles) => roles.includes(currentProjectRole.value);

const handleFilterCommand = (command) => {
  if (command === 'clear') {
    activeFilters.value.assigneeName = null
    activeFilters.value.priority = null
  } else if (command.startsWith('priority:')) {
    activeFilters.value.priority = parseInt(command.split(':')[1])
  }
}

const handleSortCommand = (command) => {
  sortBy.value = command
}

const openCreateTask = (status = 'TO DO') => {
  const statusStr = (typeof status === 'string') ? status : 'TO DO';
  newTask.value = {
    title: '',
    description: '',
    statusName: statusStr,
    priority: 3,
    assignedUserId: currentUser.id || null,
    dueDate: null
  }
  fetchProjectMembers()
  showCreateModal.value = true
}

const toggleShowCompleted = () => {
  showCompleted.value = !showCompleted.value
}

const wipLimit = ref(5) // Max 5 tasks in Progress (PM can configure later)

const updateTaskField = async (task, field, value) => {
  try {
    const updateData = {
      ...task,
      [field]: value
    }
    
    // If updating by name, reset ID to trigger backend resolution
    if (field === 'statusName') updateData.taskStatusId = '00000000-0000-0000-0000-000000000000'
    if (field === 'typeName') updateData.taskTypeId = '00000000-0000-0000-0000-000000000000'
    
    const response = await axiosClient.put(`/projects/${projectId.value}/WorkTasks/${task.id}`, updateData)
    if (response.data && response.data.data) {
      Object.assign(task, response.data.data)
    }
  } catch (error) {
    if (error.response?.status === 409) {
      ElNotification({ title: 'Conflict', message: 'Công việc đã bị thay đổi bởi người khác. Đang cập nhật lại...', type: 'warning' })
      fetchTasks()
    } else {
      console.error(`Update error for ${field}:`, error)
    }
  }
}

const toggleQuickAdd = (group) => {
  group.showQuickAdd = !group.showQuickAdd
}

const createQuickTask = async (group) => {
  if (!group.quickAddTitle) return

  try {
    const payload = {
      title: group.quickAddTitle,
      projectId: projectId.value,
      statusName: groupBy.value === 'status' ? (group.statusText || 'TO DO') : 'TO DO',
      typeName: 'Task',
      priority: groupBy.value === 'priority' ? group.priorityValue : 3
    }
    await axiosClient.post(`/projects/${projectId.value}/WorkTasks`, payload)
    await fetchTasks() // Force update
    
    if (group.id || group.showQuickAdd !== undefined) {
       group.quickAddTitle = ''
       group.showQuickAdd = false
    }
  } catch (error) {
    console.error('Quick add task error:', error)
  }
}

const handleDraggableChange = async (evt, group) => {
  if (evt.added) {
    const task = evt.added.element
    if (groupBy.value === 'status') {
       const oldStatusName = task.statusName
       // Optimistic update for the UI
       task.statusName = group.statusText
       try {
         await moveTask(task.id, null, task.rowVersion, group.statusText)
       } catch (err) {
         // Revert optimistic update on failure
         task.statusName = oldStatusName
       }
    } else if (groupBy.value === 'priority') {
       const oldPriority = task.priority
       task.priority = group.priorityValue
       try {
         await updateTaskField(task, 'priority', group.priorityValue)
       } catch (err) {
         task.priority = oldPriority
       }
    }
  }
}

const isOverdue = (date) => {
  if (!date) return false
  return new Date(date) < new Date() && !date.includes(new Date().toISOString().split('T')[0])
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN')
}

</script>

<style scoped>
/* =========================================
   LAYOUT & NAVBAR
========================================== */
/* Quick Add Input */
.quick-add-input-row {
  background-color: #1e293b50;
}
.quick-add-input {
  width: 100%;
  background: transparent;
  border: 1px solid #3b82f6;
  border-radius: 4px;
  color: #f1f5f9;
  padding: 4px 8px;
  outline: none;
}
.quick-add-input::placeholder {
  color: #64748b;
}

/* Group Popover styles (Image 3) */
.group-popover-content {
  padding: 8px 4px;
}
.popover-section-label {
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  color: #64748b;
  margin-bottom: 10px;
  padding: 0 4px;
}
.group-config-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}
.group-select-field {
  flex: 1.5;
}
.group-select-order {
  flex: 1;
}
.delete-group-icon {
  font-size: 16px;
  color: #94a3b8;
  cursor: pointer;
  padding: 8px;
}
.delete-group-icon:hover { color: #ef4444; }
.group-footer-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 4px 4px;
  border-top: 1px solid #334155;
  margin-top: 8px;
}
.footer-label { font-size: 13px; color: #cbd5e1; }

.toolbar-btn.group-active-btn {
   background-color: #5b21b630;
   border: 1px solid #7c3aed;
   color: #c084fc;
   border-radius: 20px;
   padding: 4px 12px;
}

.dashboard-layout {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-layout); 
  color: var(--text-primary); 
  overflow: hidden;
}

.top-nav {
  height: 56px;
  background-color: var(--bg-nav); 
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  width: 100%;
}

.nav-left, .nav-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 8px;
  color: white;
  text-decoration: none;
  font-weight: 800;
  font-size: 20px;
}
.nav-logo { height: 28px; }

.top-search-create {
  display: flex;
  align-items: center;
  gap: 8px; /* space between search and create */
}

.search-input-mock {
  display: flex;
  align-items: center;
  background-color: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 4px;
  padding: 0 12px;
  width: 550px;
  height: 32px;
  transition: background-color 0.2s, border-color 0.2s;
}

.search-input-mock:focus-within {
  background-color: var(--hover-bg);
  border-color: #3b82f6;
}

.search-input-mock input {
  background: transparent;
  border: none;
  color: var(--text-primary);
  font-size: 14px;
  width: 100%;
  outline: none;
}

.search-input-mock input::placeholder {
  color: #8c9bab;
}

.btn-create-jira {
  display: flex;
  align-items: center;
  gap: 6px;
  background-color: #579dff; 
  color: #1d2125;
  border: none;
  border-radius: 4px;
  padding: 0 16px;
  height: 32px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-create-jira:hover {
  background-color: #85b8ff;
}

.nav-icon {
  color: var(--text-secondary);
  font-size: 18px;
  cursor: pointer;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}
.nav-icon:hover { background-color: var(--hover-bg); color: var(--text-primary); }
.nav-icon.active { color: #3b82f6; background-color: var(--hover-bg); }

.user-avatar {
  background: #fdbba7; 
  color: #1d2125;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 14px;
  cursor: pointer;
}

/* =========================================
   SIDEBAR
========================================== */
.main-body {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.sidebar {
  width: 260px;
  background-color: var(--bg-sidebar); 
  border-right: 1px solid var(--border-color);
  padding: 24px 16px;
}

.side-menu {
  list-style: none;
  padding: 0; margin: 0;
}

.section-label {
  font-size: 11px;
  color: var(--text-muted);
  font-weight: 700;
  letter-spacing: 0.5px;
  padding: 8px 12px;
  margin-bottom: 8px;
}

.side-menu li {
  padding: 10px 12px;
  border-radius: 6px;
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: all 0.2s ease;
}
.side-menu li:hover { background-color: var(--hover-bg); color: var(--text-primary); }
.side-menu li.active { background-color: var(--active-bg); color: #60a5fa; }

.header-breadcrumbs {
  font-size: 13px;
  color: var(--text-secondary);
  font-weight: 500;
  text-decoration: underline;
  margin-bottom: 2px;
  cursor: pointer;
}

.header-main-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 4px;
}

.project-info-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.project-brand-icon {
  width: 32px;
  height: 32px;
  background: #ff5722; /* Vibrant orange-red */
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  position: relative;
  overflow: hidden;
}

.inner-icon {
  display: flex;
  flex-direction: column;
  gap: 2px;
  width: 20px;
  padding: 4px;
}

.inner-icon .line {
  height: 2px;
  background: white;
  border-radius: 1px;
}

.inner-icon .line.header { height: 3px; margin-bottom: 2px; opacity: 0.5; }
.inner-icon .line.long { width: 100%; }
.inner-icon .line.mid { width: 70%; }
.inner-icon .line.short { width: 40%; }

.page-title {
  font-size: 28px !important;
  color: var(--text-primary);
  font-weight: 700 !important;
  margin: 0 !important;
  letter-spacing: -0.5px;
}

.project-info-right {
  display: flex;
  align-items: center;
  gap: 20px;
  color: var(--text-secondary);
}

.users-icon-box {
  width: 36px;
  height: 36px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  background: var(--bg-card);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.2s;
}

.users-icon-box:hover {
  background: var(--hover-bg);
  border-color: var(--text-muted);
}

.more-icon-box .fa-ellipsis {
  font-size: 20px;
  cursor: pointer;
  color: var(--text-secondary);
  transition: color 0.1s;
}

.more-icon-box:hover {
  background: var(--hover-bg);
}

.more-icon-box:hover .fa-ellipsis {
  color: var(--text-primary);
}

/* Force Dropdown to be Dark Mode and Sync with System Colors */
:global(.el-dropdown__popper.space-settings-dropdown) {
  --el-dropdown-menu-bg-color: #1e2430 !important;
  background-color: #1e2430 !important;
  border: 1px solid #334155 !important;
  border-radius: 8px !important;
  box-shadow: 0 12px 32px rgba(0,0,0,0.8) !important;
  padding: 4px 0 !important;
}

:global(.space-settings-dropdown .el-dropdown-menu) {
  background-color: #1e2430 !important;
  padding: 4px 0 !important;
  border: none !important;
}

:global(.space-settings-dropdown .el-dropdown-menu__item) {
  color: #cbd5e1 !important;
  font-size: 14px !important;
  padding: 10px 16px !important;
  display: flex !important;
  align-items: center !important;
  gap: 12px !important;
  transition: all 0.2s !important;
}

:global(.space-settings-dropdown .el-dropdown-menu__item i) {
  font-size: 16px;
  width: 20px;
  text-align: center;
  color: #94a3b8;
}

:global(.space-settings-dropdown .el-dropdown-menu__item:hover) {
  background-color: #2c333a !important;
  color: white !important;
}

:global(.space-settings-dropdown .el-popper__arrow::before) {
  background-color: #1e2430 !important;
  border: 1px solid #334155 !important;
}

:global(.space-settings-dropdown .flex-between) {
  justify-content: space-between !important;
  width: 280px;
}

:global(.space-settings-dropdown .enterprise-badge) {
  font-size: 10px;
  font-weight: 800;
  color: #a855f7;
  border: 1px solid #a855f7;
  padding: 2px 6px;
  border-radius: 4px;
}

:global(.space-settings-dropdown .dropdown-divider) {
  height: 1px;
  background-color: #334155;
  margin: 6px 0;
}

:global(.space-settings-dropdown .danger-item) {
  color: #ff4d4f !important;
}
:global(.space-settings-dropdown .danger-item i) {
  color: #ff4d4f !important;
}

:global(.space-settings-dropdown .info-item) {
  padding: 12px 16px !important;
}

:global(.space-settings-dropdown .info-item-content) {
  display: flex;
  align-items: center;
  gap: 12px;
}

:global(.space-settings-dropdown .info-icon) {
  color: #3b82f6 !important;
  font-size: 18px !important;
}

:global(.space-settings-dropdown .info-text .primary) {
  font-weight: 600;
  color: #f1f5f9;
  font-size: 13px;
}
:global(.space-settings-dropdown .info-text .secondary) {
  color: #64748b;
  font-size: 12px;
}

.space-item-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.space-brand-icon {
  width: 24px;
  height: 24px;
  background: #ff5733;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.inner-icon {
  display: flex;
  flex-direction: column;
  gap: 2px;
  width: 14px;
}

.inner-icon .line {
  height: 2px;
  background: white;
  border-radius: 1px;
  opacity: 0.9;
}

.inner-icon .line.long { width: 100%; }
.inner-icon .line.mid { width: 70%; }
.inner-icon .line.short { width: 40%; }

.space-name {
  font-weight: 700;
  font-size: 16px;
  color: var(--text-primary);
}

.space-item-right {
  display: flex;
  align-items: center;
  gap: 12px;
  color: #8c8c8c;
}

.users-icon-box {
  width: 28px;
  height: 28px;
  border: 1px solid #334155;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  background: rgba(255, 255, 255, 0.03);
}

.space-item-right .fa-ellipsis {
  font-size: 14px;
  cursor: pointer;
}

.space-item-right .fa-ellipsis:hover {
  color: white;
}

.sub-space-item {
  padding: 12px !important;
  margin: 12px 0 16px 16px !important;
  background-color: #162032 !important;
  border: 1px solid #334155;
  border-radius: 8px !important;
}
.space-icon-small {
  width: 32px; height: 32px;
  background: linear-gradient(135deg, #a855f7, #6366f1);
  border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  font-size: 12px; font-weight: 700; color: white;
}
.space-text { display: flex; flex-direction: column; }
.space-text .name { font-size: 14px; font-weight: 600; color: white; }
.space-text .sub { font-size: 11px; color: #94a3b8; }

/* =========================================
   MAIN CONTENT
========================================== */
.content-area {
  flex: 1;
  background-color: var(--bg-content); 
  padding: 32px 40px;
  overflow-y: auto;
}
.content-wrapper { width: 100%; max-width: none; margin: 0; }

.page-title {
  font-size: 24px;
  color: #f4f5f7;
  font-weight: 600;
  margin-bottom: 24px;
}

/* Tabs */
.jira-tabs {
  display: flex;
  align-items: center;
  gap: 24px;
  border-bottom: 1px solid var(--border-color);
  margin-bottom: 24px;
}

.jira-tab {
  padding: 12px 0;
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  position: relative;
}
.jira-tab:hover { color: var(--text-primary); }
.jira-tab.active { color: #3b82f6; font-weight: 600; }
.jira-tab.active::after {
  content: ''; position: absolute; bottom: -1px; left: 0; right: 0;
  height: 2px; background-color: #3b82f6;
}

.tab-spacer { flex: 1; }
.jira-tab-icon {
  color: var(--text-secondary); font-size: 13px; cursor: pointer; display: flex; align-items: center; gap: 6px;
}

/* =========================================
   SUMMARY TAB CONTENT
========================================== */
.summary-content {
  display: flex; flex-direction: column; gap: 24px;
}
.dark-btn {
  background-color: #1e293b !important;
  color: #f1f5f9 !important;
  border-color: #334155 !important;
}

.top-widgets {
  display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px;
}
.widget {
  background-color: var(--bg-nav);
  border: 1px solid var(--border-color);
  border-radius: 8px; padding: 16px;
  display: flex; align-items: center; gap: 16px;
}
.widget-icon { font-size: 20px; }
.widget-number { font-size: 16px; font-weight: 600; color: var(--text-primary); margin-bottom: 2px;}
.widget-sub { font-size: 11px; color: var(--text-secondary); }

.charts-grid {
  display: grid; grid-template-columns: 1fr 1fr; gap: 16px;
}
.chart-card {
  background-color: var(--bg-nav);
  border: 1px solid var(--border-color);
  border-radius: 8px; padding: 20px;
}
.chart-header h4 { margin: 0 0 4px; font-size: 15px; color: var(--text-primary); }
.chart-header p { margin: 0; font-size: 12px; color: var(--text-secondary); }
.chart-header a { color: #579dff; text-decoration: none; font-size: 11px;}

.activity-overview-card { grid-column: span 1; }
.schedule-card { grid-column: span 1; }

.dark-mini-select :deep(.el-input__wrapper) {
  background-color: #1e293b !important;
  box-shadow: none !important;
  border: 1px solid #334155 !important;
}
.dark-mini-select :deep(.el-input__inner) {
  color: #94a3b8 !important;
  font-size: 11px !important;
}

.schedule-nav { display: flex; gap: 8px; color: #94a3b8; font-size: 12px;}
.schedule-nav i { 
  width: 24px; height: 24px; display: flex; align-items: center; justify-content: center;
  background: #1e293b; border-radius: 4px; cursor: pointer;
}
.schedule-nav i:hover { color: white; background: #334155; }

.schedule-body { margin-top: 20px; display: flex; flex-direction: column; gap: 40px; justify-content: center; min-height: 200px;}

.days-indicator { display: flex; justify-content: space-between; align-items: center; padding: 0 10px; }
.day-col { display: flex; flex-direction: column; align-items: center; gap: 8px; }
.day-name { font-size: 10px; color: #64748b; font-weight: 600; text-transform: uppercase; }
.day-num { 
  width: 28px; height: 38px; display: flex; align-items: center; justify-content: center;
  font-size: 14px; font-weight: 700; color: #94a3b8; border-radius: 8px; transition: all 0.2s;
}
.day-num.active { background-color: #3b82f6; color: white; box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4); }

.schedule-items { text-align: center; }
.empty-state p { font-size: 13px; color: #64748b; }

.empty-card {
  display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center;
  padding: 40px;
}
.empty-icon { font-size: 32px; color: #579dff; margin-bottom: 16px; }

.donut-body { display: flex; align-items: center; gap: 32px; margin-top: 24px; }
.donut-chart {
  position: relative; width: 120px; height: 120px; border-radius: 50%;
  background: conic-gradient(#3b82f6 0% 50%, #84cc16 50% 100%);
  display: flex; align-items: center; justify-content: center;
}
.donut-ring {
  position: absolute; width: 90px; height: 90px;
  background-color: #1e2430; border-radius: 50%;
}
.donut-center { position: relative; display: flex; flex-direction: column; align-items: center; }
.donut-center .val { font-size: 24px; font-weight: 700; color: white; }
.donut-center .lbl { font-size: 10px; color: #94a3b8; width: 60px; text-align: center; line-height: 1.2;}
.leg-item { font-size: 12px; color: #cbd5e1; margin-bottom: 8px; display: flex; align-items: center; gap: 8px; }
.dot { width: 10px; height: 10px; border-radius: 2px; }

/* Bar chart placeholder */
.bar-chart { display: flex; gap: 12px; margin-top: 24px; height: 150px; position: relative;}
.bar-y { display: flex; flex-direction: column; justify-content: space-between; font-size: 10px; color: #64748b; }
.bar-plot { flex: 1; display: flex; border-left: 1px solid #334155; border-bottom: 1px solid #334155; padding-left: 10px;}
.bar-col { flex: 1; display: flex; align-items: flex-end; justify-content: center; margin: 0 4px; position: relative; }
.bar-fill { width: 32px; border-radius: 2px 2px 0 0; }
.bar-x { position: absolute; bottom: -20px; left: 24px; right: 0; display: flex; justify-content: space-around; font-size: 9px; font-weight: 600; }

.type-chart { margin-top: 20px; }
.type-header-row { display: flex; justify-content: space-between; font-size: 11px; color: #64748b; margin-bottom: 12px; border-bottom: 1px solid #334155; padding-bottom: 8px;}
.type-row { display: flex; align-items: center; margin-bottom: 16px; }
.t-name { width: 120px; font-size: 13px; color: #cbd5e1; display: flex; align-items: center; gap: 8px;}
.t-dist { flex: 1; background-color: #334155; height: 16px; border-radius: 4px; overflow: hidden; }
.t-prog { background-color: #64748b; height: 100%; display: flex; align-items: center; padding-left: 8px; font-size: 10px; color: white; transition: width 0.5s;}

/* =========================================
   LIST TAB CONTENT (From previous step)
========================================== */
.list-tab-wrapper { display: flex; flex-direction: column; width: 100%; }

.list-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}
.toolbar-left, .toolbar-right { display: flex; align-items: center; gap: 12px; }
.toolbar-btn {
  display: flex; align-items: center; gap: 8px;
  background-color: transparent; border: 1px solid var(--border-color); border-radius: 20px;
  padding: 6px 12px; color: var(--text-secondary); font-size: 13px; cursor: pointer; transition: all 0.2s;
}
.toolbar-btn:hover { background-color: var(--hover-bg); color: var(--text-primary);}
.toolbar-btn.primary-tint { background-color: #3b0764; border-color: #6b21a8; color: #d8b4fe; }
.toolbar-icon { color: var(--text-secondary); cursor: pointer; padding: 0 8px; font-size: 14px;}
.avatar-tiny { background:#f8fafc; color:#0c101a; border-radius:50%; width:16px; height:16px; display:inline-flex; align-items:center; justify-content:center; font-size:10px; font-weight:bold; margin-left:4px; }
.add-task-white-btn {
  background-color: #f8fafc; color: #0f172a; border: none; border-radius: 6px;
  padding: 8px 16px; font-weight: 600; font-size: 13px; display: flex; align-items: center; cursor: pointer;
}

.list-view-container { display: flex; flex-direction: column; gap: 32px; }
.task-group { border-bottom: 1px solid #1e293b; padding-bottom: 24px; }
.group-header { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; padding-left: 8px;}
.toggle-icon { color: #a1a1aa; cursor: pointer; font-size: 14px; transition: transform 0.2s; }
.group-badge { display: flex; align-items: center; gap: 6px; padding: 4px 10px; border-radius: 4px; font-weight: 700; font-size: 11px; }
.group-count { color: var(--text-secondary); font-size: 13px; }

.list-row { display: flex; align-items: center; padding: 0 16px; border-bottom: 1px dashed #27272a;}
.header-row { color: var(--text-secondary); font-size: 12px; height: 36px; border-bottom: none !important;}
.task-row { height: 48px; transition: background 0.2s; cursor: pointer; background-color: transparent; border-bottom: 1px solid var(--border-color);}
.task-row:hover { background-color: var(--hover-bg); }

.nav-center {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}
.col-name { display: flex; align-items: center; gap: 12px; color: var(--text-primary); font-size: 14px; font-weight: 500; padding-left: 24px; flex: 1; min-width: 250px;}
.col-assignee { width: 120px; font-size: 13px; color: var(--text-secondary); }
.col-date { width: 120px; font-size: 13px; color: var(--text-secondary); }
.col-priority { width: 100px; font-size: 14px; }
.col-status { width: 140px; }
.col-comments { width: 80px; text-align: left; color: var(--text-secondary); font-size: 14px; }
.col-add { width: 40px; text-align: right; color: var(--text-secondary); font-size: 14px;}

.check-icon { font-size: 16px; color: #52525b; }
.icon-btn { color: #a1a1aa; font-size: 14px; cursor: pointer;}
.icon-btn:hover { color: #f8fafc; }
.status-btn { display: inline-flex; padding: 4px 10px; border-radius: 4px; font-size: 10px; line-height: 1; gap: 6px; align-items: center;}

.add-task-row { height: 40px; cursor: pointer; border-bottom: none; color: #a1a1aa;}
.add-task-row:hover { color: #f8fafc; }
.add-task-row .col-name { padding-left: 24px; font-size: 13px; font-weight: 400;}

/* =========================================
   AI SIDEBAR (Right)
========================================== */
.ai-sidebar {
  width: 320px;
  background-color: #131824;
  border-left: 1px solid #1e293b;
  display: flex;
  flex-direction: column;
}

@media (max-width: 1200px) {
  .ai-sidebar {
    position: fixed;
    right: 0;
    top: 56px;
    bottom: 0;
    z-index: 1000;
  }
}

@media (max-width: 900px) {
  .search-input-mock {
    width: 100% !important;
  }
}

@media (max-width: 768px) {
  .sidebar {
    position: fixed;
    left: -260px;
    top: 56px;
    bottom: 0;
    z-index: 1001;
    transition: left 0.3s ease;
  }
  .sidebar.show {
    left: 0;
  }
  .content-area {
    padding: 20px 16px;
  }
  .top-widgets {
    grid-template-columns: 1fr 1fr;
  }
  .charts-grid {
    grid-template-columns: 1fr;
  }
  .nav-center {
    display: none;
  }
  .nav-left, .nav-right {
    width: auto;
  }
  .jira-tabs {
    overflow-x: auto;
    padding-bottom: 8px;
  }
  .jira-tab {
    white-space: nowrap;
  }
}

.mobile-only {
  display: none;
}

@media (max-width: 768px) {
  .mobile-only {
    display: flex;
  }
  .desktop-only {
    display: none;
  }
}

.menu-toggle {
  font-size: 20px;
  color: #94a3b8;
  cursor: pointer;
  margin-right: 12px;
}

.ai-header {
  padding: 16px 20px;
  border-bottom: 1px solid #1e293b;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.ai-header h4 { font-size: 14px; display: flex; align-items: center; gap: 8px; margin: 0; color: #f8fafc;}
.ai-header h4 i { color: #579dff; }

.ai-content { flex: 1; padding: 24px 20px; overflow-y: auto; }
.quick-actions-title { font-size: 10px; font-weight: 600; color: #64748b; margin-bottom: 12px; letter-spacing: 0.5px;}
.quick-actions {
  /* Dynamically change chips based on tab */
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 32px;
}
.quick-actions-col { display: flex; flex-direction: column; gap: 8px; margin-bottom: 32px; }
.ai-chip {
  background-color: transparent; border: 1px solid #334155; color: #cbd5e1;
  padding: 6px 12px; border-radius: 20px; font-size: 12px; cursor: pointer;
  transition: all 0.2s;
}
.ai-chip:hover { background-color: #1e293b; color: white; border-color: #475569; }

.action-btn {
  background-color: transparent; border: 1px solid #334155; color: #cbd5e1;
  padding: 10px 16px; border-radius: 8px; font-size: 12px; cursor: pointer;
  display: flex; align-items: center; gap: 10px; transition: all 0.2s;
}
.action-btn:hover { background-color: #1e293b; color: white; border-color: #475569; }

.chat-message { display: flex; gap: 12px; margin-bottom: 20px; }
.avatar-bot {
  width: 28px; height: 28px; border-radius: 50%; background-color: #3b82f6; color: white;
  display: flex; align-items: center; justify-content: center; font-size: 14px;
}
.message-bubble {
  flex: 1; background-color: #1e293b; padding: 14px 16px; border-radius: 4px 16px 16px 16px;
  color: #e2e8f0; font-size: 13px; line-height: 1.5;
}

.ai-input-area { padding: 20px; border-top: 1px solid #1e293b; }
.ai-input-wrapper { background-color: #1e293b; border-radius: 12px; display: flex; flex-direction: column; overflow: hidden; }
.ai-input-wrapper input { background: transparent; border: none; padding: 16px; color: white; font-size: 13px; outline: none; }
.ai-input-actions { display: flex; align-items: center; padding: 4px 16px 12px; gap: 12px; }
.ai-input-actions i { color: #64748b; cursor: pointer; transition: color 0.1s;}
.ai-input-actions i:hover { color: white; }
.send-btn { background: #3b82f6; border: none; width: 28px; height: 28px; border-radius: 50%; color: white; display: flex; align-items: center; justify-content: center; margin-left: auto; cursor: pointer; }
.send-btn i { font-size: 12px; }

/* Animations */
.slide-right-enter-active, .slide-right-leave-active { transition: transform 0.3s ease; }
.slide-right-enter-from, .slide-right-leave-to { transform: translateX(100%); }

/* Comment Popover Specific - Dùng :global để chắc chắn tác động được vào Element teleported popper */
:global(.el-popper.comment-popover-dark) {
  background-color: #1d2125 !important;
  border: 1px solid #334155 !important;
  padding: 0 !important;
  border-radius: 8px !important;
  box-shadow: 0 12px 32px rgba(0,0,0,0.6) !important;
}

:global(.comment-popover-dark .el-popper__arrow::before) {
  background-color: #1d2125 !important;
  border: 1px solid #334155 !important;
}

.comment-trigger-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}
.comment-trigger-btn:hover .icon-btn { color: white; }
.comment-count-text { font-size: 13px; font-weight: 700; color: #cbd5e1; }

.comment-popover-content {
  display: flex;
  flex-direction: column;
  max-height: 500px;
}

.comments-scroll-area {
  padding: 20px;
  overflow-y: auto;
  max-height: 300px;
  border-bottom: 1px solid #2c333a;
  background-color: #1d2125;
}

.comment-item {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.comment-user {
  display: flex;
  gap: 12px;
  align-items: center;
}
.avatar-circle {
  width: 32px;
  height: 32px;
  background-color: #475569;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  color: white;
}
.user-meta {
  display: flex;
  align-items: baseline;
  gap: 8px;
}
.user-name { font-size: 14px; font-weight: 700; color: var(--text-primary); }
.time-stamp { font-size: 11px; color: var(--text-muted); }

.comment-text {
  font-size: 14px;
  color: var(--text-secondary);
  padding-left: 44px;
}

.comment-actions-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-left: 44px;
  margin-top: 4px;
}
.left-actions { display: flex; gap: 16px; color: #64748b; font-size: 14px; }
.left-actions i:hover { color: white; cursor: pointer; }
.answer-link { font-size: 12px; font-weight: 600; color: #cbd5e1; cursor: pointer; }
.answer-link:hover { color: white; }

.comment-input-section {
  padding: 16px 20px;
  background-color: var(--bg-secondary);
  border-radius: 0 0 8px 8px;
}

.input-upper textarea {
  width: 100%;
  background: transparent;
  border: none;
  color: #cbd5e1;
  font-size: 14px;
  resize: none;
  height: 48px;
  outline: none;
  font-family: inherit;
}

.input-lower {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 12px;
}

.lower-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.tool-btn {
  background-color: #222;
  border-radius: 6px;
  padding: 6px 10px;
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}
.tool-btn.circle { border-radius: 50%; width: 28px; height: 28px; justify-content: center; padding: 0; }
.tool-btn.comment-type { font-size: 12px; font-weight: 600; color: #cbd5e1; }
.tool-btn:hover { background-color: #333; }

.tool-icon {
  color: #64748b;
  font-size: 14px;
  cursor: pointer;
  transition: color 0.2s;
}
.tool-icon:hover { color: white; }

.send-icon {
  color: #475569;
  font-size: 18px;
  cursor: pointer;
}
.send-icon:hover { color: #579dff; }


/* TASK DETAIL MODAL */
.task-modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(2px);
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
}

.task-modal {
  width: 95%;
  max-width: 1300px;
  height: 90vh;
  background-color: var(--bg-card);
  border-radius: 12px;
  border: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0,0,0,0.5);
}

.modal-header {
  height: 48px;
  background-color: var(--bg-layout);
  border-bottom: 1px solid var(--border-color);
  padding: 0 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.header-left { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #94a3b8; }
.header-left i { font-size: 14px; }
.m-crumb { cursor: pointer; }
.m-crumb:hover { color: white; }
.m-crumb.current { color: #f1f5f9; font-weight: 600; }
.separator { font-size: 10px; color: #475569; }
.add-crumb, .copy-crumb { color: #64748b; margin-left: 4px; cursor: pointer; font-size: 12px; }

.header-right { display: flex; align-items: center; gap: 16px; color: #64748b; }
.created-at { font-size: 12px; }
.btn-ai-header { background: #3b0764; color: #d8b4fe; padding: 4px 10px; border-radius: 4px; font-size: 12px; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 6px; }
.btn-share { background: #222; color: #cbd5e1; padding: 4px 10px; border-radius: 4px; font-size: 12px; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 6px; }
.m-more, .m-fav, .m-pin, .m-side-toggle, .m-close { cursor: pointer; font-size: 14px; transition: color 0.1s; }
.m-more:hover, .m-fav:hover, .m-close:hover { color: white; }

.modal-body-wrapper { flex: 1; display: flex; overflow: hidden; }

/* Left Section */
.modal-main { flex: 1; padding: 40px; overflow-y: auto; background-color: var(--bg-card); }
.task-id-row { display: flex; align-items: center; gap: 12px; margin-bottom: 24px; }
.status-badge-small { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; }
.task-id-text { font-size: 12px; color: var(--text-muted); font-family: monospace; }
.btn-ai-mini { font-size: 12px; color: #579dff; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 6px; }

.task-modal-title { font-size: 32px; font-weight: 700; color: var(--text-primary); margin-bottom: 24px; }

.ai-prompt-bar {
  background-color: #1a1a1a;
  border-radius: 8px;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 12px;
  border: 1px solid #333;
  margin-bottom: 40px;
}
.sparkle-icon { color: #a855f7; font-size: 16px; }
.ai-prompt-bar input { background: transparent; border: none; flex: 1; color: #94a3b8; font-size: 14px; outline: none; }

.attributes-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px 40px;
  margin-bottom: 40px;
}
.attr-item { display: flex; flex-direction: column; gap: 4px; }
.attr-label { font-size: 12px; font-weight: 700; color: #64748b; display: flex; align-items: center; gap: 8px; }
.attr-label i { width: 14px; text-align: center; }
.attr-value { font-size: 14px; color: #e2e8f0; display: flex; align-items: center; gap: 10px; }
.attr-value.muted { color: #475569; }

.status-pill {
  background-color: #3b0764; color: #d8b4fe; padding: 4px 10px; border-radius: 4px;
  font-size: 12px; font-weight: 800; display: flex; align-items: center; gap: 8px; cursor: pointer;
}
.check-confirm { color: #22c55e; font-size: 14px; }

.content-section { display: flex; flex-direction: column; gap: 16px; margin-bottom: 60px; }
.section-link { color: #64748b; font-size: 14px; display: flex; align-items: center; gap: 10px; cursor: pointer; }
.section-link:hover { color: #f1f5f9; }
.section-link.ai-link { color: #a855f7; font-weight: 600; }

.fields-section h3 { font-size: 16px; font-weight: 600; color: #64748b; margin-bottom: 16px; }
.add-field-btn { background: transparent; border: 1px dashed #334155; color: #475569; padding: 12px 20px; border-radius: 8px; cursor: pointer; font-size: 13px; text-align: left; }
.add-field-btn:hover { border-color: #64748b; color: #94a3b8; }

/* Right Sidebar */
.modal-sidebar {
  width: 440px;
  background-color: var(--bg-card);
  border-left: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  padding: 24px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #1a1a1a;
}
.sidebar-header h2 { font-size: 18px; font-weight: 700; color: #f1f5f9; }
.header-tools { display: flex; align-items: center; gap: 16px; color: #64748b; font-size: 14px; }
.header-tools i { cursor: pointer; }

.activity-scroll { flex: 1; padding: 20px; overflow-y: auto; }
.comment-card { margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px solid #1a1a1a;}
.c-head { display: flex; align-items: center; gap: 10px; margin-bottom: 8px; }
.avatar-sm { width: 24px; height: 24px; background: #64748b; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 10px; font-weight: 700; color: white; }
.c-user { font-size: 13px; font-weight: 700; color: #f1f5f9; }
.c-time { font-size: 11px; font-weight: 400; color: #475569; margin-left: 6px; }
.c-body { font-size: 14px; color: #cbd5e1; padding-left: 34px; line-height: 1.5; }
.c-foot { display: flex; align-items: center; justify-content: space-between; padding-left: 34px; margin-top: 12px; }
.c-actions { display: flex; gap: 12px; color: #64748b; font-size: 12px; }
.c-rep { font-size: 12px; font-weight: 600; color: #64748b; cursor: pointer; transition: color 0.2s; }
.c-rep:hover { color: #3b82f6; }

.replies-container {
  margin-top: 12px;
  margin-left: 17px;
  position: relative;
  border-left: 2px solid #30363d;
}

.replies-container::before {
  content: '';
  position: absolute;
  top: -12px;     /* Start slightly above the container */
  left: -2px;     /* Align with border-left */
  width: 2px;
  height: 12px;   /* Connects to parent comment */
  background-color: #30363d;
}

.reply-card, .inline-reply-box {
  margin-bottom: 12px;
  margin-left: 32px; /* Giving space from vertical line */
  position: relative;
}

.reply-card:last-child, .inline-reply-box:last-child {
  margin-bottom: 0;
}

.reply-card::before, .inline-reply-box::before {
  content: '';
  position: absolute;
  top: -24px;   /* Start high up to connect to the main line */
  left: -34px;  /* Reach across the gap (32px + 2px border) */
  width: 22px;
  height: 38px;
  border-bottom: 2px solid #30363d;
  border-left: 2px solid #30363d;
  border-bottom-left-radius: 12px;
  pointer-events: none;
}

.inline-reply-box {
  display: flex;
  gap: 8px;
  align-items: flex-start;
  margin-top: 12px;
}

.inline-input-wrapper {
  flex: 1;
  background: #161b22;
  border: 1px solid #30363d;
  border-radius: 12px;
  padding: 8px 12px;
}

.inline-input-wrapper textarea {
  width: 100%;
  background: transparent;
  border: none;
  color: #c9d1d9;
  font-size: 13px;
  resize: none;
  min-height: 24px;
  outline: none;
}

.inline-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 4px;
}
.inline-actions i {
  color: #64748b;
  cursor: pointer;
  font-size: 14px;
}
.inline-actions i.send-enabled { color: #3b82f6; }
.inline-actions i.cancel-btn:hover { color: #ef4444; }

.activity-input { padding: 20px; background-color: var(--bg-card); border-top: 1px solid var(--border-color); }
.input-container { background-color: var(--bg-secondary); border: 1px solid var(--border-color); border-radius: 8px; display: flex; flex-direction: column; }
.input-container textarea { background: transparent; border: none; padding: 12px 16px; color: var(--text-primary); font-size: 13px; resize: none; min-height: 48px; outline: none; }

.input-actions-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  border-top: 1px solid var(--border-color);
}
.bar-left { display: flex; align-items: center; gap: 12px; color: #64748b; font-size: 12px; }
.bar-left i { cursor: pointer; }
.bar-left i.ai { color: #a855f7; }
.btn-comment-type { background: #222; border: none; border-radius: 4px; color: #cbd5e1; font-size: 11px; padding: 4px 8px; display: flex; align-items: center; gap: 4px; cursor: pointer;}

.bar-right { display: flex; align-items: center; gap: 8px; color: #475569; }
.send-enabled { color: #3b82f6; font-size: 16px; cursor: pointer; }

/* =========================================
   CALENDAR VIEW
========================================== */
.calendar-content {
  padding: 0 10px;
  height: 100%;
}

.calendar-header-toolbar {
  display: flex;
  align-items: center;
  margin-bottom: 24px;
}

.calendar-month-title {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
  margin-right: 24px;
}

.calendar-nav-controls {
  margin-right: auto;
}

.btn-group-jira {
  display: flex;
  background-color: var(--bg-secondary);
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid var(--border-color);
}

.jira-control-btn {
  background: transparent !important;
  border: none !important;
  border-right: 1px solid var(--border-color) !important;
  color: var(--text-secondary) !important;
  margin: 0 !important;
  height: 32px !important;
  padding: 0 12px !important;
  border-radius: 0 !important;
}

.jira-control-btn:last-child {
  border-right: none !important;
}

.jira-control-btn:hover {
  background-color: #2c333a !important;
  color: white !important;
}

.today-btn {
  font-weight: 600 !important;
}

.toggle-group-jira {
  display: flex;
  background-color: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 4px;
  padding: 2px;
}

.toggle-item {
  padding: 4px 16px;
  font-size: 13px;
  font-weight: 600;
  color: #94a3b8;
  cursor: pointer;
  border-radius: 3px;
  transition: all 0.2s;
}

.toggle-item.active {
  background-color: #3b82f6;
  color: white;
}

.calendar-grid-container {
  border: 1px solid var(--border-color);
  border-radius: 6px;
  overflow: hidden;
  background-color: var(--bg-layout);
}

.calendar-week-labels {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  background-color: var(--bg-layout);
  border-bottom: 1px solid var(--border-color);
}

.weekday-label {
  padding: 10px;
  text-align: left;
  font-size: 11px;
  font-weight: 800;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.calendar-days-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  grid-auto-rows: minmax(120px, auto);
}

.day-cell {
  border-right: 1px solid var(--border-color);
  border-bottom: 1px solid var(--border-color);
  padding: 10px;
  color: var(--text-primary);
  font-size: 13px;
  font-weight: 500;
  position: relative;
}

.day-cell:nth-child(7n) {
  border-right: none;
}

.day-cell.muted {
  color: #334155;
}

.day-cell.today {
  background-color: rgba(59, 130, 246, 0.05);
}

.day-num-circle {
  width: 24px;
  height: 24px;
  background-color: #3b82f6;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  margin-bottom: 8px;
}

.calendar-event {
  margin-top: 8px;
  padding: 6px 8px;
  border-radius: 4px;
  font-size: 11px;
  display: flex;
  flex-direction: column;
  gap: 2px;
  cursor: pointer;
}

.calendar-event.blue { background-color: #1e3a8a; border-left: 3px solid #3b82f6; }
.calendar-event.orange { background-color: #78350f; border-left: 3px solid #f59e0b; }
.calendar-event.green { background-color: #064e3b; border-left: 3px solid #10b981; }

.event-id { font-weight: 700; color: #60a5fa; opacity: 0.9; }
.event-name { color: #e2e8f0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* =========================================
   TIMELINE VIEW (ROADMAP)
========================================== */
.timeline-content {
  padding: 0 10px;
  display: flex;
  flex-direction: column;
  height: calc(100vh - 200px);
}

.timeline-toolbar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.timeline-search {
  display: flex;
  align-items: center;
  background-color: var(--bg-layout);
  border: 1px solid var(--border-color);
  border-radius: 4px;
  padding: 0 10px;
  width: 200px;
  height: 32px;
}

.timeline-search i { font-size: 14px; color: #8c9bab; margin-right: 8px; }
.timeline-search input { background: transparent; border: none; color: white; width: 100%; outline: none; font-size: 13px; }

.avatar-circle-sm {
  width: 24px;
  height: 24px;
  background-color: #f59e0b;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
  color: #1d2125;
}

.toolbar-right-icons {
  margin-left: auto;
  display: flex;
  gap: 16px;
  color: #8c9bab;
}

.timeline-grid-wrapper {
  flex: 1;
  display: flex;
  border: 1px solid var(--border-color);
  border-radius: 5px;
  overflow: hidden;
  background-color: var(--bg-card);
}

.timeline-left-panel {
  width: 260px;
  border-right: 1px solid #1e293b;
  display: flex;
  flex-direction: column;
}

.panel-header {
  padding: 12px 16px;
  font-size: 13px;
  font-weight: 700;
  color: var(--text-primary);
  background-color: var(--bg-layout);
  border-bottom: 1px solid var(--border-color);
}

.panel-sub-header {
  padding: 8px 16px;
  font-size: 11px;
  font-weight: 700;
  color: var(--text-secondary);
  text-transform: uppercase;
}

.epic-row {
  padding: 10px 16px;
  font-size: 13px;
  color: #94a3b8;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  border-bottom: 1px solid rgba(255,255,255,0.03);
}

.epic-row:hover { background-color: #1e293b; color: white; }

.add-epic-btn {
  padding: 12px 16px;
  color: #8c9bab;
  font-size: 13px;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.add-epic-btn:hover { color: white; }

.timeline-main-grid {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-x: auto;
}

.timeline-months-row {
  display: flex;
  background-color: var(--bg-layout);
  border-bottom: 1px solid var(--border-color);
}

.month-col {
  min-width: 200px;
  flex: 1;
  padding: 12px;
  font-size: 12px;
  font-weight: 500;
  color: var(--text-secondary);
  text-align: center;
  border-right: 1px solid var(--border-color);
}

.timeline-rows-container {
  flex: 1;
  position: relative;
}

.timeline-task-row {
  height: 48px;
  display: flex;
  border-bottom: 1px solid var(--border-color);
  position: relative;
}

.grid-line {
  min-width: 200px;
  flex: 1;
  border-right: 1px solid var(--border-color);
  height: 100%;
}

.gantt-bar {
  position: absolute;
  top: 12px;
  height: 24px;
  border-radius: 4px;
  cursor: pointer;
}

.gantt-bar.purple { background-color: #7c3aed; }
.gantt-bar.blue { background-color: #2563eb; }
.gantt-bar.green { background-color: #10b981; }

.timeline-bottom-nav {
  position: fixed;
  bottom: 24px;
  right: 480px; /* Adjust based on AI sidebar */
  background-color: #22272b;
  border: 1px solid #334155;
  border-radius: 6px;
  padding: 4px;
  display: flex;
  align-items: center;
  box-shadow: 0 4px 12px rgba(0,0,0,0.5);
  z-index: 50;
}

.bottom-nav-group {
  display: flex;
  border-right: 1px solid #334155;
}

.bottom-item {
  padding: 4px 12px;
  font-size: 12px;
  font-weight: 600;
  color: #94a3b8;
  cursor: pointer;
  border-radius: 3px;
}

.bottom-item.active {
  background-color: #3b82f633;
  color: #60a5fa;
}

.bottom-nav-icons {
  padding: 0 12px;
  display: flex;
  gap: 12px;
  color: #8c9bab;
  font-size: 12px;
}

/* =========================================
   PAGES VIEW (ENHANCED)
========================================== */
.pages-content {
  height: calc(100vh - 180px);
  overflow: hidden;
}

.pages-layout-wrapper {
  display: flex;
  height: 100%;
}

.pages-main-area {
  flex: 1;
  padding: 0 40px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.pages-header-section {
  margin-bottom: 24px;
}

.pages-title {
  font-size: 24px;
  font-weight: 700;
  color: #f1f5f9;
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.pages-desc {
  font-size: 14px;
  color: #94a3b8;
}

/* Editor Window Mockup */
.editor-window {
  background-color: var(--bg-card);
  border-radius: 8px 8px 0 0;
  border: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  flex: 1;
  box-shadow: 0 10px 40px rgba(0,0,0,0.1);
}

.editor-window-header {
  height: 32px;
  background-color: var(--bg-layout);
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  padding: 0 16px;
}

.header-dots {
  display: flex;
  gap: 6px;
}

.header-dots span {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: #444;
}

.editor-toolbar-expanded {
  padding: 8px 16px;
  background-color: #1d2125;
  border-bottom: 1px solid #333c43;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
}

.t-left, .t-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

.t-icon {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #8c9bab;
  cursor: pointer;
  border-radius: 4px;
  font-size: 14px;
}

.t-icon:hover { background-color: #333c43; color: white; }
.t-icon.active { color: #579dff; background-color: rgba(87, 157, 255, 0.1); }

.t-divider {
  width: 1px;
  height: 20px;
  background-color: #333c43;
}

.t-dropdown {
  padding: 0 12px;
  height: 32px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #8c9bab;
  font-size: 13px;
  cursor: pointer;
  border-radius: 4px;
}
.t-dropdown:hover { background-color: #333c43; }

.saving-text {
  font-size: 11px;
  color: #64748b;
  margin-right: 8px;
}

.t-avatar-circle {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #f59e0b;
  color: #1d2125;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
}

.publish-btn { background-color: #579dff !important; border: none !important; font-weight: 600 !important; }
.close-btn { background-color: #333c43 !important; border: none !important; color: #f1f5f9 !important; font-weight: 600 !important; }

.editor-content-area {
  padding: 60px 100px;
  color: var(--text-primary);
}

.editor-main-heading { font-size: 32px; font-weight: 700; margin-bottom: 24px; }
.editor-main-p { font-size: 16px; color: #94a3b8; line-height: 1.6; margin-bottom: 24px; }
.editor-tip { font-size: 14px; color: #94a3b8; }
.mention-tag { background-color: var(--bg-secondary); color: var(--text-secondary); padding: 2px 6px; border-radius: 4px; font-size: 12px; }

/* Template Sidebar */
.pages-template-sidebar {
  width: 300px;
  padding: 24px;
  background-color: var(--bg-card);
  border-left: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
}

.template-sidebar-title { font-size: 16px; font-weight: 700; color: #f1f5f9; margin-bottom: 4px; }
.template-sidebar-subtitle { font-size: 11px; font-weight: 800; color: #64748b; margin-bottom: 24px; letter-spacing: 0.5px; }

.template-list { display: flex; flex-direction: column; gap: 8px; }

.template-item {
  display: flex;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}

.template-item:hover { background-color: #1e293b; }
.template-item.active { background-color: rgba(59, 130, 246, 0.15); border: 1px solid #3b82f633; }

.template-icon {
  width: 32px;
  height: 32px;
  min-width: 32px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
}

.template-icon.blue { background-color: #0c4a6e; color: #38bdf8; }
.template-icon.purple { background-color: #4c1d95; color: #a78bfa; }
.template-icon.green { background-color: #064e3b; color: #34d399; }
.template-icon.light-blue { background-color: #1e3a8a; color: #60a5fa; }
.template-icon.yellow { background-color: #713f12; color: #fbbf24; }

.template-name { font-size: 13px; font-weight: 700; color: #f1f5f9; margin-bottom: 2px; }
.template-desc { font-size: 12px; color: #64748b; line-height: 1.4; }

.explore-more {
  margin-top: auto;
  padding: 16px;
  font-size: 13px;
  color: #8c9bab;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
}
.explore-more:hover { color: white; }

/* Footer Bar */
.pages-footer-bar {
  background-color: #22272b;
  border: 1px solid #333c43;
  padding: 12px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 24px;
  border-radius: 6px 6px 0 0;
  font-size: 13px;
  color: #94a3b8;
}

.footer-actions { display: flex; align-items: center; gap: 16px; }
.learn-more { cursor: pointer; color: #8c9bab; }
.learn-more:hover { text-decoration: underline; }
.try-now-btn { background-color: #3b82f6 !important; border: none !important; font-weight: 600 !important; }

/* =========================================
   FORMS VIEW
========================================== */
.forms-content {
  padding: 60px 40px;
  max-width: 900px;
  margin: 0 auto;
  text-align: center;
}

.forms-headline {
  font-size: 36px;
  font-weight: 800;
  color: white;
  margin-bottom: 24px;
}

.forms-subheadline {
  font-size: 16px;
  color: #94a3b8;
  line-height: 1.6;
  max-width: 600px;
  margin: 0 auto 60px;
}

.forms-workflow-visual {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 32px;
  margin-bottom: 80px;
}

.visual-card {
  width: 180px;
  height: 160px;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
}

.visual-icon {
  font-size: 32px;
  color: #579dff;
}

.visual-icon.active { color: #579dff; }

.visual-label {
  font-size: 14px;
  font-weight: 700;
  color: #f1f5f9;
}

.visual-arrow { color: #333c43; font-size: 24px; }

.forms-features-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
  text-align: left;
  margin-bottom: 60px;
}

.feature-card {
  padding-left: 16px;
  border-left: 2px solid #3b82f6;
}

.feature-title {
  font-size: 18px;
  font-weight: 700;
  color: #f1f5f9;
  margin-bottom: 8px;
}

.feature-desc {
  font-size: 14px;
  color: #94a3b8;
  line-height: 1.5;
}

.forms-cta-section { margin-top: 40px; }

.create-form-btn {
  padding: 12px 24px !important;
  font-weight: 700 !important;
  background-color: #3b82f6 !important;
  border: none !important;
}





/* =========================================
   BOARD VIEW (KANBAN)
========================================== */
.board-content {
  padding: 0 10px;
}

.board-toolbar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
}

.board-search {
  display: flex;
  align-items: center;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 4px;
  padding: 0 10px;
  width: 240px;
  height: 32px;
}

.board-search i {
  color: #8c9bab;
  font-size: 14px;
  margin-right: 8px;
}

.board-search input {
  background: transparent;
  border: none;
  color: #f4f5f7;
  font-size: 14px;
  width: 100%;
  outline: none;
}

.board-avatars {
  display: flex;
  align-items: center;
}

.avatar-stack {
  display: flex;
  align-items: center;
}

.stack-item {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 2px solid #1d2125;
  margin-left: -8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  color: white;
  cursor: pointer;
}

.stack-item:first-child { margin-left: 0; }

.stack-count {
  margin-left: 8px;
  color: #8c9bab;
  font-size: 13px;
  font-weight: 500;
}

.board-grouping {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #8c9bab;
}

.sprint-btn {
  background-color: #0b2149 !important;
  border-color: #0b2149 !important;
  color: #579dff !important;
  font-weight: 600 !important;
  margin-left: auto;
}

.kanban-board {
  display: flex;
  gap: 12px;
  height: calc(100vh - 280px);
  overflow-x: auto;
  align-items: flex-start;
  padding-bottom: 10px;
}

.kanban-column {
  background-color: var(--bg-layout);
  width: 280px;
  min-width: 280px;
  border-radius: 8px;
  padding: 12px 10px;
  display: flex;
  flex-direction: column;
}

.column-header {
  display: flex;
  align-items: center;
  padding: 8px 8px 12px;
  color: #8c9bab;
}

.ch-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.todo-dot { color: #94a3b8; font-size: 10px; }
.progress-spin { color: #3b82f6; font-size: 12px; }
.review-eye { color: #f59e0b; font-size: 12px; }
.done-check { color: #22c55e; font-size: 12px; }

.column-title {
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: #94a3b8;
}

.column-count-badge {
  background-color: #333c43;
  color: #f1f5f9;
  font-size: 11px;
  font-weight: 700;
  padding: 2px 7px;
  border-radius: 12px;
}

.header-more {
  margin-left: auto;
  font-size: 14px;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
}
.header-more:hover { background-color: rgba(255,255,255,0.1); color: white; }

.btn-create-card-column {
  padding: 10px;
  color: #8c9bab;
  font-size: 14px;
  cursor: pointer;
  border-radius: 6px;
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 8px;
  transition: all 0.2s;
}

.btn-create-card-column:hover {
  background-color: rgba(255, 255, 255, 0.08);
  color: #f4f5f7;
}

.kanban-cards {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-height: calc(100vh - 350px);
}

.jira-empty-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 10px;
  text-align: center;
  margin-top: 20px;
}
.empty-icon-wrap {
  margin-bottom: 24px;
}
.jira-empty-col h4 {
  font-size: 16px;
  font-weight: 700;
  color: #f1f5f9;
  margin: 0 0 8px 0;
}
.jira-empty-col p {
  font-size: 13px;
  color: #94a3b8;
  margin: 0 0 24px 0;
}
.btn-go-backlog {
  background-color: var(--bg-secondary);
  border: 1px solid var(--border-color);
  color: #f1f5f9;
  padding: 8px 20px;
  border-radius: 4px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}
.btn-go-backlog:hover {
  background-color: #334155;
  border-color: #64748b;
}

.kanban-card {
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 4px;
  padding: 12px;
  cursor: pointer;
  box-shadow: 0 1px 3px rgba(0,0,0,0.3);
  transition: background 0.2s;
}

.kanban-card:hover {
  background-color: var(--hover-bg);
}

.kanban-card.active-card {
  background-color: #2c333a;
  border-color: #579dff;
}

.card-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}

.card-title {
  font-size: 15px;
  color: #f4f5f7;
  flex: 1;
  margin: 0;
  font-weight: 500;
}

.edit-icon, .more-icon {
  font-size: 14px;
  color: #8c9bab;
  opacity: 0.6;
}
.edit-icon:hover, .more-icon:hover { opacity: 1; color: white; }

.card-badges {
  margin-bottom: 16px;
}

.date-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 8px;
  background-color: rgba(255, 255, 255, 0.05);
  border: 1px solid #333c43;
  border-radius: 4px;
  font-size: 11px;
  color: #cbd5e1;
}

.date-badge.overdue {
  background-color: rgba(239, 68, 68, 0.1);
  border-color: #ef4444;
  color: #f87171;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
}

.card-task-id {
  font-size: 12px;
  color: #8c9bab;
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 500;
}

.type-icon {
  font-size: 14px;
}

.footer-right {
  display: flex;
  align-items: center;
}

.assignee-avatar {
  background-color: #3b82f6 !important;
  color: white !important;
  font-weight: 700;
}

.avatar-circle-xs {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background-color: #475569;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  color: white;
}

/* Inline Date Picker */
.inline-date-picker {
  width: 120px !important;
}
:deep(.el-input__wrapper) {
  background-color: transparent !important;
  box-shadow: none !important;
  padding: 0 !important;
}
:deep(.el-input__inner) {
  color: #94a3b8 !important;
  font-size: 12px;
}
:deep(.el-input__prefix) {
  display: none;
}

.btn-create-card {
  padding: 8px 10px;
  color: #8c9bab;
  font-size: 13px;
  cursor: pointer;
  border-radius: 4px;
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 4px;
}

.btn-create-card:hover {
  background-color: rgba(255,255,255,0.05);
  color: #f4f5f7;
}

.add-column-box {
  width: 40px;
  min-width: 40px;
  display: flex;
  justify-content: center;
}

.add-column-btn {
  width: 40px;
  height: 40px;
  background-color: #161a1d;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  color: #8c9bab;
  cursor: pointer;
  transition: all 0.2s;
  margin-top: 0;
}

.add-column-btn:hover {
  background-color: #2c333a;
  color: white;
}

/* AI SIDEBAR */
.ai-sidebar {
  width: 420px;
  background-color: var(--bg-card);
  border-left: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  height: 100%;
}

.ai-header {
  padding: 24px;
  border-bottom: 1px solid var(--border-color);
}

.ai-header h4 {
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.ai-header h4 i {
  color: #60a5fa; 
}

.ai-content {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
}

.quick-actions-title {
  font-size: 11px;
  font-weight: 700;
  color: #64748b;
  margin-bottom: 12px;
  text-align: center;
}

.quick-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: center;
  margin-bottom: 32px;
}

.ai-chip {
  background-color: transparent !important;
  color: #94a3b8 !important;
  border-color: #334155 !important;
  font-size: 12px !important;
}

.ai-chip:hover {
  background-color: #1e3a8a !important;
  color: #60a5fa !important;
  border-color: #60a5fa !important;
}

.chat-message {
  margin-bottom: 20px;
}

.message-bubble {
  background-color: var(--bg-layout);
  padding: 16px;
  border-radius: 12px 12px 12px 0;
  color: var(--text-primary);
  font-size: 14px;
  line-height: 1.5;
  margin-bottom: 8px;
}

.message-meta {
  font-size: 11px;
  color: #64748b;
  padding-left: 4px;
}

.ai-input-area {
  padding: 20px;
  background-color: var(--bg-card);
  border-top: 1px solid var(--border-color);
}

.ai-input-wrapper {
  background-color: var(--bg-layout);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.ai-input-wrapper input {
  background: transparent;
  border: none;
  padding: 16px;
  color: white;
  font-size: 14px;
  outline: none;
}

.ai-input-wrapper input::placeholder {
  color: #64748b;
}

.ai-input-actions {
  display: flex;
  align-items: center;
  padding: 8px 16px 12px;
  gap: 12px;
}

.ai-input-actions i {
  color: #64748b;
  cursor: pointer;
  transition: color 0.1s;
}

.ai-input-actions i:hover {
  color: white;
}

.send-btn {
  background: #3b82f6;
  border: none;
  width: 28px;
  height: 28px;
  border-radius: 6px;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: auto;
  cursor: pointer;
  transition: opacity 0.2s;
}

.send-btn:hover {
  opacity: 0.9;
}

.send-btn i {
  color: white !important;
  font-size: 12px;
}

.slide-right-enter-active, .slide-right-leave-active { transition: transform 0.3s ease; }
.slide-right-enter-from, .slide-right-leave-to { transform: translateX(100%); }

.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
/* Teams Dialog Styling */
:global(.jira-dark-dialog.teams-dialog) {
  background-color: #2c333a !important;
  border-radius: 8px !important;
  border: 1px solid #444c54 !important;
  padding: 0 !important;
  overflow: hidden !important;
}

:global(.teams-dialog .el-dialog__header) {
  display: none !important;
}

:global(.teams-dialog .el-dialog__body) {
  padding: 0 !important;
}

.teams-dialog-header {
  height: 180px;
  background-color: var(--bg-layout);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.header-illustration {
  width: 100%;
  height: 100%;
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding-top: 20px;
}

.team-banner {
  background: var(--bg-card);
  border-radius: 12px;
  padding: 10px 16px;
  display: flex;
  align-items: center;
  gap: 40px;
  border: 1px solid var(--border-color);
  box-shadow: 0 8px 16px rgba(0,0,0,0.2);
  z-index: 2;
  position: relative;
}

.team-tag {
  display: flex;
  align-items: center;
  gap: 12px;
  color: #f1f5f9;
  font-weight: 600;
  font-size: 14px;
}

.tag-icon {
  width: 28px;
  height: 28px;
  background: #84cc16; 
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
}

.team-avatars {
  display: flex;
  margin-left: -20px;
}

.avatar-ring {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 2px solid #161a1d;
  overflow: hidden;
  margin-left: -8px;
  transition: transform 0.2s;
}
.avatar-ring img { width: 100%; height: 100%; object-fit: cover; }
.av-1 { border-color: #c084fc; }
.av-2 { border-color: #3b82f6; }
.av-3 { border-color: #84cc16; }
.av-4 { border-color: #f59e0b; }

.mini-cards {
  display: flex;
  gap: 12px;
  margin-top: 12px;
  opacity: 0.6;
}

.m-card {
  background: #161a1d;
  border-radius: 8px;
  padding: 8px 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100px;
  border: 1px solid #333;
}

.m-card .c-icon { font-size: 14px; }
.m-card .c-icon.blue { color: #3b82f6; }
.m-card .c-icon.purple { color: #a855f7; }
.m-card .c-icon.gray { color: #64748b; }

.m-card .c-line {
  height: 2px;
  flex: 1;
  background: #333;
  border-radius: 1px;
}

.sparkles {
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  pointer-events: none;
}
.sparkles i { position: absolute; color: #fff; opacity: 0.4; }
.sp-1 { top: 20%; right: 20%; font-size: 12px; }
.sp-2 { top: 15%; right: 25%; font-size: 8px; }
.sp-3 { top: 30%; right: 15%; font-size: 10px; }

.teams-dialog-body {
  padding: 24px 32px;
}

.dialog-main-title {
  font-size: 20px !important;
  color: #f1f5f9 !important;
  margin-bottom: 8px !important;
  font-weight: 600 !important;
}

.dialog-subtitle {
  color: var(--text-secondary) !important;
  font-size: 14px !important;
  line-height: 1.5 !important;
  margin-bottom: 24px !important;
}

.search-teams-box {
  background-color: var(--bg-layout);
  border: 2px solid var(--border-color);
  border-radius: 4px;
  display: flex;
  align-items: center;
  padding: 10px 16px;
  gap: 12px;
  transition: border-color 0.2s;
}

.search-teams-box:focus-within {
  border-color: #579dff;
}

.search-teams-box i { color: #8c9bab; font-size: 16px; }
.search-teams-box input {
  background: transparent;
  border: none;
  color: white;
  flex: 1;
  font-size: 14px;
  outline: none;
}

.dialog-footer {
  padding: 0 32px 32px;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.cancel-btn { color: #cbd5e1 !important; font-weight: 600; }
.cancel-btn:hover { color: white !important; }

.save-btn {
  background-color: #579dff !important;
  color: #1d2125 !important;
  font-weight: 600 !important;
  border: none !important;
  padding: 8px 24px !important;
}
.save-btn:hover { background-color: #85b8ff !important; }
/* =========================================
   BACKLOG VIEW
========================================== */
.backlog-content {
  padding: 24px;
  background-color: var(--bg-layout);
  min-height: 100%;
}
.backlog-header-jira {
  margin-bottom: 24px;
}
.backlog-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}
.muted-text {
  color: var(--text-secondary);
  font-size: 14px;
}
.backlog-list-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.backlog-group {
  background-color: var(--bg-card);
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid var(--border-color);
}
.backlog-group-header {
  padding: 8px 16px;
  background-color: var(--bg-layout);
  display: flex;
  align-items: center;
  gap: 12px;
  color: var(--text-primary);
  font-size: 13px;
  font-weight: 600;
}
/* Custom Charts UI additions */
.custom-legend-body {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 24px;
  padding-bottom: 20px;
}
.custom-status-legend {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.legend-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: var(--text-secondary);
  cursor: pointer;
  position: relative;
}
.legend-item:hover {
  color: var(--text-primary);
}
.l-color {
  width: 14px;
  height: 14px;
  border-radius: 2px;
}
.l-tooltip {
  position: absolute;
  top: 24px;
  left: 0;
  background: var(--bg-layout);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  padding: 8px 12px;
  border-radius: 4px;
  font-size: 12px;
  white-space: nowrap;
  z-index: 1000;
  box-shadow: 0 4px 12px rgba(0,0,0,0.3);
  pointer-events: none;
}
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

.type-progress-list {
  padding: 0 16px 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.type-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.type-header-row {
  font-size: 13px;
  color: var(--text-muted);
  font-weight: 600;
  margin-bottom: 4px;
}
.t-col-name {
  width: 130px;
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  color: var(--text-primary);
}
.t-col-name i {
  font-size: 16px;
  width: 16px;
  text-align: center;
}
.t-col-bar {
  flex: 1;
}
.p-bar-bg {
  height: 28px;
  background-color: var(--border-color);
  border-radius: 4px;
  width: 100%;
  position: relative;
}
.p-bar-bg.empty {
  background-color: #334155 !important;
}
.p-bar-fill {
  height: 100%;
  background-color: #64748b;
  display: flex;
  align-items: center;
  border-radius: 4px;
  transition: width 0.4s ease;
}
.p-bar-text {
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  padding-left: 10px;
}
.p-bar-text.text-outside {
  color: var(--text-primary);
  position: absolute;
  left: calc(100% + 8px);
}
.view-all-link {
  color: #3b82f6;
  text-decoration: none;
  font-weight: 500;
}
.view-all-link:hover {
  text-decoration: underline;
}

.bg-header-count {
  background-color: var(--bg-secondary);
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 11px;
  color: var(--text-secondary);
}
.backlog-items-area {
  min-height: 40px;
}
.backlog-item-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 16px;
  border-bottom: 1px solid var(--border-color);
  cursor: pointer;
  transition: background-color 0.1s;
}
.backlog-item-row:hover {
  background-color: #33415580;
}
.bi-left {
  display: flex;
  align-items: center;
  gap: 12px;
}
.bi-key {
  color: var(--text-secondary);
  font-size: 12px;
  font-weight: 600;
  min-width: 70px;
}
.bi-title {
  color: var(--text-primary);
  font-size: 14px;
}
.bi-right {
  display: flex;
  align-items: center;
  gap: 16px;
}
.prio-tag {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
}
.prio-1 { color: #ef4444; background: #ef444420; }
.prio-2 { color: #f97316; background: #f9731620; }
.prio-3 { color: #3b82f6; background: #3b82f620; }
.prio-4 { color: #94a3b8; background: #94a3b820; }

.avatar-circle-xs {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #475569;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
}

.create-task-form {
  padding: 10px 0;
}
.create-task-form .form-item {
  margin-bottom: 20px;
}
.create-task-form .form-item label {
  display: block;
  font-weight: 600;
  margin-bottom: 8px;
  color: #c1c7d0;
  font-size: 13px;
}
.create-task-form .form-row {
  display: flex;
  gap: 20px;
}
.create-task-form .form-row .form-item.half {
  flex: 1;
}

/* Custom Dialog Dark Theme */
:deep(.create-task-dialog) {
  background: #1d2125 !important;
  border-radius: 8px;
}
:deep(.create-task-dialog .el-dialog__title) {
  color: #f4f5f7;
}
:deep(.create-task-dialog .el-dialog__body) {
  color: #c1c7d0;
  padding: 10px 20px;
}
:deep(.create-task-dialog .el-input__wrapper),
:deep(.create-task-dialog .el-textarea__wrapper),
:deep(.create-task-dialog .el-select .el-input__wrapper) {
  background-color: #22272b !important;
  box-shadow: 0 0 0 1px #454f59 inset !important;
}
:deep(.create-task-dialog .el-input__inner),
:deep(.create-task-dialog .el-textarea__inner) {
  color: #f4f5f7 !important;
}
:deep(.create-task-dialog .el-dialog__footer) {
  border-top: 1px solid #454f59;
  padding: 12px 20px 20px;
}

.avatar-tiny {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: #3b82f6;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.sidebar-more-trigger {
  padding: 10px 12px;
  border-radius: 6px;
  color: #cbd5e1;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: all 0.2s;
  width: 100%;
  box-sizing: border-box;
}

.sidebar-more-trigger i {
  font-size: 16px;
  width: 20px;
  text-align: center;
}

.sidebar-more-trigger:hover {
  background-color: var(--hover-bg);
  color: var(--text-primary);
}
</style>

<style>
.custom-sidebar-dropdown.el-popper {
  background: var(--bg-card) !important;
  border: 1px solid var(--border-color) !important;
  border-radius: 4px !important;
}
.custom-sidebar-dropdown .el-dropdown-menu__item {
  background-color: transparent !important;
  color: var(--text-primary) !important;
}
.custom-sidebar-dropdown .el-dropdown-menu__item:hover,
.custom-sidebar-dropdown .el-dropdown-menu__item:focus {
  background-color: var(--hover-bg) !important;
}
.custom-sidebar-dropdown .el-popper__arrow::before {
  background: var(--bg-card) !important;
  border: 1px solid var(--border-color) !important;
}
</style>
