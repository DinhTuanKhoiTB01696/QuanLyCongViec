<template>
  <section class="forms-tab">
    <div class="forms-copy">
      <h2>{{ t('workItems.forms.title') }}</h2>
      <p>
        {{ t('workItems.forms.permissionText') }}
        <button
          type="button"
          class="admin-link"
          disabled
          :title="t('workItems.forms.contactAdminNeedsFlow')"
        >
          {{ t('workItems.forms.contactAdmin') }}
        </button>
        {{ t('workItems.forms.permissionSuffix') }}
      </p>
    </div>

    <div class="forms-visual" aria-hidden="true">
      <div class="form-card submit-card">
        <h3>{{ t('workItems.forms.submitForm') }}</h3>
        <span></span>
        <span></span>
        <span></span>
      </div>

      <div class="flow-arrow">
        <i class="fa-solid fa-arrow-right"></i>
      </div>

      <div class="form-card track-card">
        <h3>{{ t('workItems.forms.trackRequests') }}</h3>
        <div class="mini-table">
          <span v-for="cell in tableCells" :key="cell"></span>
        </div>
      </div>

      <div class="caption left">
        <strong>{{ t('workItems.forms.captureKeyDetails') }}</strong>
        <span>{{ t('workItems.forms.raiseRequests') }}</span>
      </div>
      <div class="caption right">
        <strong>{{ t('workItems.forms.prioritizeWork') }}</strong>
        <span>{{ t('workItems.forms.onBoardOrList') }}</span>
      </div>
    </div>
  </section>
</template>

<script setup>
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()
const tableCells = Array.from({ length: 12 }, (_, index) => index)
</script>

<style scoped>
.forms-tab {
  flex: 1;
  min-height: 0;
  overflow: auto;
  padding: 40px 36px;
  background: var(--color-bg);
  color: var(--color-text-primary);
}

.forms-copy {
  max-width: 720px;
}

.forms-copy h2 {
  margin: 0 0 8px;
  color: var(--color-text-primary);
  font-size: 24px;
  font-weight: 700;
  letter-spacing: 0;
}

.forms-copy p {
  margin: 0;
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 1.5;
}

.admin-link {
  border: 0;
  background: transparent;
  color: #0c66e4;
  padding: 0;
  font: inherit;
  vertical-align: baseline;
}

.admin-link:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.forms-visual {
  position: relative;
  width: min(560px, 100%);
  height: 300px;
  margin: 54px auto 0;
}

.form-card {
  position: absolute;
  width: 150px;
  min-height: 124px;
  border: 1px solid #dfe1e6;
  background: var(--color-surface);
  box-shadow: 0 10px 22px rgba(9, 30, 66, 0.12);
  padding: 16px 14px;
  z-index: 1;
}

.form-card h3 {
  margin: 0 0 16px;
  color: var(--color-text-primary);
  font-size: 14px;
  font-weight: 700;
}

.submit-card {
  left: 118px;
  top: 12px;
}

.submit-card::before {
  content: "";
  position: absolute;
  inset: -11px -8px;
  z-index: -1;
  transform: rotate(-4deg);
  background: #ffab00;
}

.submit-card span {
  display: block;
  height: 16px;
  margin-bottom: 12px;
  border: 1px solid #dfe1e6;
  background:
    linear-gradient(105deg, transparent 0 22%, #c1c7d0 22% 25%, transparent 25% 43%, #c1c7d0 43% 46%, transparent 46%),
    #ffffff;
}

.track-card {
  right: 118px;
  top: 16px;
}

.track-card::before {
  content: "";
  position: absolute;
  inset: -8px -14px;
  z-index: -1;
  border-radius: 50%;
  background: #86bc25;
}

.mini-table {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 5px;
}

.mini-table span {
  height: 10px;
  border-radius: 2px;
  background: #dfe1e6;
}

.mini-table span:nth-child(4n + 2) {
  background: #e3fcef;
}

.mini-table span:nth-child(4n + 3) {
  border-radius: 50%;
  width: 10px;
  justify-self: center;
  background: #c1c7d0;
}

.mini-table span:nth-child(4n) {
  background: #deebff;
}

.flow-arrow {
  position: absolute;
  left: 254px;
  top: 56px;
  width: 64px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #172b4d;
  font-size: 36px;
  transform: rotate(15deg);
  z-index: 2;
}

.flow-arrow::before,
.flow-arrow::after {
  content: "";
  position: absolute;
  width: 16px;
  height: 2px;
  border-radius: 99px;
  background: #172b4d;
}

.flow-arrow::before {
  left: -14px;
  top: -8px;
  transform: rotate(80deg);
}

.flow-arrow::after {
  right: -10px;
  top: -2px;
  transform: rotate(-65deg);
}

.caption {
  position: absolute;
  top: 210px;
  width: 180px;
  text-align: center;
  color: var(--color-text-secondary);
  font-size: 12px;
  line-height: 1.35;
}

.caption strong {
  display: block;
  color: var(--color-text-primary);
  font-size: 13px;
}

.caption.left {
  left: 98px;
}

.caption.right {
  right: 94px;
}

@media (max-width: 720px) {
  .forms-tab {
    padding: 28px 20px;
  }

  .forms-copy h2 {
    font-size: 20px;
  }

  .forms-visual {
    height: auto;
    display: grid;
    grid-template-columns: 1fr;
    gap: 18px;
    margin-top: 32px;
  }

  .form-card,
  .flow-arrow,
  .caption {
    position: static;
    width: min(220px, 100%);
    margin: 0 auto;
  }

  .flow-arrow {
    height: 28px;
    transform: none;
    font-size: 24px;
  }

  .caption {
    width: min(260px, 100%);
  }
}
</style>
