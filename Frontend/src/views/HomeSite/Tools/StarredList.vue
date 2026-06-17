<template>
  <div class="starred-page">
    <header class="page-header">
      <h1>Có gắn sao</h1>
    </header>

    <div class="page-content">
      <div class="filter-controls">
        <div class="search-input-wrapper">
          <i class="fa-solid fa-magnifying-glass search-icon"></i>
          <input type="text" placeholder="Tìm kiếm theo tiêu đề" class="search-input" />
        </div>
        <button class="filter-btn">
          <i class="fa-solid fa-sliders"></i> Lọc theo ứng dụng
        </button>
      </div>

      <div class="empty-state">
        <div class="empty-illustration">
          <div class="medal-icon">
            <div class="medal-circle">
              <i class="fa-regular fa-star"></i>
            </div>
            <div class="medal-ribbons">
              <div class="ribbon left-ribbon"></div>
              <div class="ribbon right-ribbon"></div>
            </div>
          </div>
        </div>
        
        <h2>Dù chưa có sao nhưng đã sẵn sàng để tỏa sáng</h2>
        <p>Bắt đầu thêm dấu sao vào các mục quan trọng đối với bạn như các trang, blog, dự án và mục tiêu. Bạn sẽ thấy những nội dung này ở đây để dễ dàng truy cập.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
// Minimal logic for the UI representation
</script>

<style scoped>
.starred-page {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  color: #172B4D;
  background-color: #FFFFFF;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.page-header {
  padding: 32px 40px 0;
}

.page-header h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0 0 24px 0;
}

.page-content {
  padding: 0 40px 40px;
  border-top: 1px solid #DFE1E6;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.filter-controls {
  display: flex;
  gap: 12px;
  padding: 24px 0;
}

.search-input-wrapper {
  position: relative;
  width: 240px;
}

.search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: #5E6C84;
  font-size: 14px;
}

.search-input {
  width: 100%;
  padding: 8px 12px 8px 44px;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  background-color: #FFFFFF;
  color: #091E42;
  font-size: 14px;
  outline: none;
  transition: background-color 0.2s, border-color 0.2s;
  box-sizing: border-box;
}

.search-input:hover {
  background-color: #FAFBFC;
}

.search-input:focus {
  background-color: #FFFFFF;
  border-color: #4C9AFF;
}

.filter-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  background: transparent;
  border: 1px solid #DFE1E6;
  padding: 8px 12px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  color: #42526E;
  cursor: pointer;
  transition: background-color 0.2s;
}

.filter-btn:hover {
  background: #FAFBFC;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  max-width: 500px;
  margin: 0 auto;
  text-align: center;
  padding-top: 40px;
}

.empty-illustration {
  margin-bottom: 24px;
}

.medal-icon {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.medal-circle {
  width: 80px;
  height: 80px;
  background-color: #FFC400;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2;
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}

.medal-circle i {
  font-size: 36px;
  color: #172B4D;
}

.medal-ribbons {
  display: flex;
  gap: 4px;
  margin-top: -15px;
  z-index: 1;
}

.ribbon {
  width: 24px;
  height: 40px;
  background-color: #0052CC;
}

.left-ribbon {
  transform: rotate(30deg);
  margin-right: 15px;
  clip-path: polygon(0 0, 100% 0, 100% 100%, 50% 80%, 0 100%);
}

.right-ribbon {
  transform: rotate(-30deg);
  margin-left: 15px;
  clip-path: polygon(0 0, 100% 0, 100% 100%, 50% 80%, 0 100%);
}

.empty-state h2 {
  font-size: 20px;
  font-weight: 600;
  color: #172B4D;
  margin: 0 0 16px 0;
}

.empty-state p {
  font-size: 14px;
  color: #5E6C84;
  line-height: 1.5;
  margin: 0;
}
</style>
