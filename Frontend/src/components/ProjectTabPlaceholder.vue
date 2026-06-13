<template>
  <!--
    Placeholder dùng chung cho các tab CHƯA có ảnh chứng minh nội dung:
    Summary, Development, Forms, Docs.
    Theo nguyên tắc: không bịa nội dung khi thiếu bằng chứng -> hiển thị [CẦN XÁC NHẬN].
  -->
  <div class="tab-placeholder">
    <div class="ph-box">
      <i :class="icon" class="ph-icon"></i>
      <h3>{{ title }}</h3>
      <span class="ph-flag">[CẦN XÁC NHẬN]</span>
      <p>
        Chưa có ảnh/mô tả nội dung cho tab <strong>{{ title }}</strong> trong tài liệu đã gửi.
        Cần bổ sung ảnh hoặc đặc tả để dựng đúng UI, tránh tự suy đoán.
      </p>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  icon: { type: String, default: 'fa-solid fa-circle-question' }
})
</script>

<style scoped>
.tab-placeholder { flex: 1; display: flex; align-items: center; justify-content: center; padding: 48px; background: var(--color-bg); }
.ph-box { max-width: 420px; text-align: center; border: 1px dashed var(--color-border); border-radius: 8px; padding: 40px 32px; }
.ph-icon { font-size: 28px; color: var(--color-text-muted); margin-bottom: 12px; }
.ph-box h3 { margin: 0 0 8px; color: var(--color-text-primary); font-size: 18px; }
.ph-flag { display: inline-block; background: #fff0b3; color: #974f0c; font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 3px; margin-bottom: 12px; }
.ph-box p { color: var(--color-text-muted); font-size: 13px; line-height: 1.6; margin: 0; }
</style>
