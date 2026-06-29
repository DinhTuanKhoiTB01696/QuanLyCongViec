const fs = require('fs');
const file = 'Frontend/src/views/HomeSite/Projects/ProjectDetail.vue';
const content = fs.readFileSync(file, 'utf8');

const learningsRegex = /<\!-- Tab: Bài học rút ra -->[\s\S]*?<\!-- Tab: Rủi ro -->/;
const learningsTemplate = `<!-- Tab: Bài học rút ra -->
        <template v-if="currentTab === 'learnings'">
          <div v-if="!editing.learnings && !projectLessons.length" class="empty-state-large-tab">
            <div class="empty-illustration">
              <i class="fa-solid fa-lightbulb" style="color: #0052CC; font-size: 64px;"></i>
            </div>
            <div class="empty-text-content">
              <h4>Những bộ óc vĩ đại có tư duy giống nhau sẽ chia sẻ kiến thức của họ</h4>
              <p>Chia sẻ những gì bạn đã học được với công ty của bạn để giúp những người khác có một khởi đầu thuận lợi khi làm việc trên các dự án tương tự.</p>
              <div class="empty-actions">
                <button class="secondary-btn" @click="editing.learnings = true">Thêm bài học rút ra mới</button>
              </div>
            </div>
          </div>
          <div v-else>
            <div v-if="editing.learnings" class="tab-item-editor" style="margin-bottom: 24px; padding-top: 16px;">
                <RichTextEditor v-model="newProjectItem.text" @save="saveProjectLesson" @cancel="cancelProjectItem('learnings')" placeholder="Dùng không gian này để chia sẻ bài học rút ra...">
                  <template #header>
                    <div style="display: flex; align-items: center; gap: 12px; padding: 12px 16px; border-bottom: 1px solid #DFE1E6; background-color: #fff;">
                      <i class="fa-regular fa-lightbulb" style="color: #FFAB00; font-size: 18px;"></i>
                      <input type="text" v-model="newProjectItem.title" placeholder="Tóm tắt cho bài học rút ra của bạn là gì?" style="border: none; outline: none; background: transparent; width: 100%; font-size: 15px; font-weight: 500; color: #172B4D;" />
                    </div>
                  </template>
                </RichTextEditor>
            </div>
            <div v-if="!editing.learnings && projectLessons.length" style="margin-bottom: 24px; padding-top: 16px;">
                <button class="secondary-btn" @click="editing.learnings = true">Thêm bài học</button>
            </div>
            
            <div class="timeline-post" v-for="item in projectLessons" :key="item.id" style="margin-bottom: 16px;">
                <div class="post-header">
                  <div class="post-user">
                    <UserAvatar :user="{ id: item.creatorId, fullName: item.creatorName, avatarUrl: item.creatorAvatarUrl, email: item.creatorEmail }" :size="32" :fontSize="12" />
                    <div class="user-info">
                      <span class="user-name">{{ item.creatorName }}</span>
                      <span class="post-time">{{ formatDate(item.createdAt) }}</span>
                    </div>
                  </div>
                </div>
                <div class="post-content">
                  <h4 style="margin: 0 0 8px 0; color: #172B4D; font-size: 16px;"><i class="fa-regular fa-lightbulb" style="color: #FFAB00; margin-right: 6px;"></i> {{ item.title }}</h4>
                  <div v-html="sanitizeHtml(item.text)"></div>
                </div>
            </div>
          </div>
        </template>

        <!-- Tab: Rủi ro -->`;

let newContent = content.replace(learningsRegex, learningsTemplate);

const risksRegex = /<\!-- Tab: Rủi ro -->[\s\S]*?<\!-- Tab: Quyết định -->/;
const risksTemplate = `<!-- Tab: Rủi ro -->
        <template v-if="currentTab === 'risks'">
          <div v-if="!editing.risks && !projectRisks.length" class="empty-state-large-tab">
            <div class="empty-illustration">
              <i class="fa-solid fa-triangle-exclamation" style="color: #FF5630; font-size: 64px;"></i>
            </div>
            <div class="empty-text-content">
              <h4>Nắm bắt các rủi ro đã biết</h4>
              <p>Theo dõi mọi rủi ro liên quan đến dự án này để tránh những bất ngờ sau này.</p>
              <div class="empty-actions">
                <button class="secondary-btn" @click="editing.risks = true">Thêm rủi ro mới</button>
              </div>
            </div>
          </div>
          <div v-else>
            <div v-if="editing.risks" class="tab-item-editor" style="margin-bottom: 24px; padding-top: 16px;">
                <RichTextEditor v-model="newProjectItem.text" @save="saveProjectRisk" @cancel="cancelProjectItem('risks')" placeholder="Mô tả rủi ro...">
                  <template #header>
                    <div style="display: flex; align-items: center; gap: 12px; padding: 12px 16px; border-bottom: 1px solid #DFE1E6; background-color: #fff;">
                      <i class="fa-solid fa-triangle-exclamation" style="color: #FF5630; font-size: 18px;"></i>
                      <input type="text" v-model="newProjectItem.title" placeholder="Tóm tắt rủi ro là gì?" style="border: none; outline: none; background: transparent; width: 100%; font-size: 15px; font-weight: 500; color: #172B4D;" />
                    </div>
                  </template>
                </RichTextEditor>
            </div>
            <div v-if="!editing.risks && projectRisks.length" style="margin-bottom: 24px; padding-top: 16px;">
                <button class="secondary-btn" @click="editing.risks = true">Thêm rủi ro</button>
            </div>
            
            <div class="timeline-post" v-for="item in projectRisks" :key="item.id" style="margin-bottom: 16px;">
                <div class="post-header">
                  <div class="post-user">
                    <UserAvatar :user="{ id: item.creatorId, fullName: item.creatorName, avatarUrl: item.creatorAvatarUrl, email: item.creatorEmail }" :size="32" :fontSize="12" />
                    <div class="user-info">
                      <span class="user-name">{{ item.creatorName }}</span>
                      <span class="post-time">{{ formatDate(item.createdAt) }}</span>
                    </div>
                  </div>
                </div>
                <div class="post-content">
                  <h4 style="margin: 0 0 8px 0; color: #172B4D; font-size: 16px;"><i class="fa-solid fa-triangle-exclamation" style="color: #FF5630; margin-right: 6px;"></i> {{ item.title }}</h4>
                  <div v-html="sanitizeHtml(item.text)"></div>
                </div>
            </div>
          </div>
        </template>

        <!-- Tab: Quyết định -->`;

newContent = newContent.replace(risksRegex, risksTemplate);

const decisionsRegex = /<\!-- Tab: Quyết định -->[\s\S]*?<\/div>\s*<\!-- Right Column: Sidebar Details -->/;
const decisionsTemplate = `<!-- Tab: Quyết định -->
        <template v-if="currentTab === 'decisions'">
          <div v-if="!editing.decisions && !projectDecisions.length" class="empty-state-large-tab">
            <div class="empty-illustration">
              <i class="fa-solid fa-check-circle" style="color: #36B37E; font-size: 64px;"></i>
            </div>
            <div class="empty-text-content">
              <h4>Truyền đạt các quyết định lớn</h4>
              <p>Ghi lại các quyết định lớn cho dự án này tại đây để chia sẻ trong bản cập nhật mới nhất của bạn.</p>
              <div class="empty-actions">
                <button class="secondary-btn" @click="editing.decisions = true">Thêm quyết định mới</button>
              </div>
            </div>
          </div>
          <div v-else>
            <div v-if="editing.decisions" class="tab-item-editor" style="margin-bottom: 24px; padding-top: 16px;">
                <RichTextEditor v-model="newProjectItem.text" @save="saveProjectDecision" @cancel="cancelProjectItem('decisions')" placeholder="Mô tả quyết định...">
                  <template #header>
                    <div style="display: flex; align-items: center; gap: 12px; padding: 12px 16px; border-bottom: 1px solid #DFE1E6; background-color: #fff;">
                      <i class="fa-solid fa-check-circle" style="color: #36B37E; font-size: 18px;"></i>
                      <input type="text" v-model="newProjectItem.title" placeholder="Tóm tắt quyết định là gì?" style="border: none; outline: none; background: transparent; width: 100%; font-size: 15px; font-weight: 500; color: #172B4D;" />
                    </div>
                  </template>
                </RichTextEditor>
            </div>
            <div v-if="!editing.decisions && projectDecisions.length" style="margin-bottom: 24px; padding-top: 16px;">
                <button class="secondary-btn" @click="editing.decisions = true">Thêm quyết định</button>
            </div>
            
            <div class="timeline-post" v-for="item in projectDecisions" :key="item.id" style="margin-bottom: 16px;">
                <div class="post-header">
                  <div class="post-user">
                    <UserAvatar :user="{ id: item.creatorId, fullName: item.creatorName, avatarUrl: item.creatorAvatarUrl, email: item.creatorEmail }" :size="32" :fontSize="12" />
                    <div class="user-info">
                      <span class="user-name">{{ item.creatorName }}</span>
                      <span class="post-time">{{ formatDate(item.createdAt) }}</span>
                    </div>
                  </div>
                </div>
                <div class="post-content">
                  <h4 style="margin: 0 0 8px 0; color: #172B4D; font-size: 16px;"><i class="fa-solid fa-check-circle" style="color: #36B37E; margin-right: 6px;"></i> {{ item.title }}</h4>
                  <div v-html="sanitizeHtml(item.text)"></div>
                </div>
            </div>
          </div>
        </template>

      </div>
      <!-- Right Column: Sidebar Details -->`;

newContent = newContent.replace(decisionsRegex, decisionsTemplate);

fs.writeFileSync(file, newContent);
