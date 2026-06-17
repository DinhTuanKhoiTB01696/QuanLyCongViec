<template>
  <div class="teams-dashboard">
    <header class="page-header">
      <h1>Teams</h1>
    </header>
    
    <div class="tabs-nav">
      <div class="tab active">Dành cho bạn</div>
      <div class="tab">Nhóm</div>
      <div class="tab">Khen ngợi</div>
      <div class="tab">Mọi người</div>
    </div>

    <div class="dashboard-content">
      <section class="section-block">
        <h2>Teams for you</h2>
        <p class="empty-state">No teams assigned yet. Join or create a team to get started.</p>
      </section>

      <section class="section-block">
        <h2>Recent Kudos</h2>
        <p class="empty-state">No kudos to display.</p>
      </section>
    </div>
  </div>
</template>

<style scoped>
.page-header h1 {
  font-size: 24px;
  font-weight: 600;
  margin: 0 0 24px 0;
  color: #172b4d;
}

.tabs-nav {
  display: flex;
  border-bottom: 2px solid #ebecf0;
  margin-bottom: 32px;
}

.tab {
  padding: 8px 16px;
  font-size: 14px;
  font-weight: 500;
  color: #5e6c84;
  cursor: pointer;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
}

.tab:hover {
  color: #172b4d;
}

.tab.active {
  color: #0052cc;
  border-bottom-color: #0052cc;
}

.section-block {
  margin-bottom: 40px;
}

.section-block h2 {
  font-size: 16px;
  font-weight: 600;
  margin: 0 0 16px 0;
  color: #172b4d;
}

.empty-state {
  padding: 32px;
  text-align: center;
  background-color: #fafbfc;
  border: 1px dashed #dfe1e6;
  border-radius: 3px;
  color: #5e6c84;
  font-size: 14px;
}
</style>
