const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN/QuanLyCongViec/Frontend/src/views/HomeSite/Projects/ProjectDetail.vue';
let content = fs.readFileSync(file, 'utf8');

const lines = content.split(/\r?\n/);

const startIdx = lines.findIndex(l => l.includes('<!-- Chủ sở hữu -->'));
const endIdx = lines.findIndex((l, i) => i > startIdx && l.includes('<!-- Người theo dõi -->')) - 1;

if (startIdx !== -1 && endIdx !== -1) {
    const replacement = `          <!-- Chủ sở hữu -->
          <div class="detail-row">
            <div class="detail-label">Chủ sở hữu</div>
            <div class="detail-value">
              <div class="owner-chip" v-if="project.leadUserId">
                <div class="owner-avatar-micro" style="background-color: #0052CC; color: white;">
                  {{ project.leadName ? project.leadName.charAt(0).toUpperCase() : 'U' }}
                </div>
                <span class="owner-name">{{ project.leadName }}</span>
              </div>
              <div class="owner-chip" v-else>
                <div class="owner-avatar-micro" style="background-color: #DFE1E6; color: #5E6C84;">?</div>
                <span class="owner-name" style="color: #5E6C84;">Chưa có</span>
              </div>
            </div>
          </div>
          
          <!-- Người đóng góp -->
          <div class="detail-row">
            <div class="detail-label">Người đóng góp <span class="badge-count">{{ contributors.length }}</span> <button class="icon-btn-micro"><i class="fa-solid fa-plus"></i></button></div>
            <div class="detail-value">
              <div class="owner-chip" v-for="c in contributors" :key="c.id">
                <div class="owner-avatar-micro" style="background-color: #36B37E; color: white;">
                  {{ c.name.charAt(0).toUpperCase() }}
                </div>
                <div class="owner-info">
                  <span class="owner-name">{{ c.name }}</span>
                  <span class="owner-role">Người đóng góp</span>
                </div>
              </div>
              <div v-if="contributors.length === 0" style="font-size: 13px; color: #5E6C84; margin-top: 4px;">Chưa có người đóng góp</div>
            </div>
          </div>`;
          
    lines.splice(startIdx, endIdx - startIdx + 1, replacement);
    fs.writeFileSync(file, lines.join('\r\n'), 'utf8');
    console.log('Success');
} else {
    console.log('Failed to find range', startIdx, endIdx);
}
