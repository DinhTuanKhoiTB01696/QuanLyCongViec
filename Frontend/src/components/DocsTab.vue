<template>
  <section class="docs-tab" aria-labelledby="docs-tab-title">
    <div class="docs-connect-banner">
      <div class="docs-illustration" aria-hidden="true">
        <div class="doc-window">
          <div class="doc-bar"></div>
          <div class="doc-line doc-line-long"></div>
          <div class="doc-line doc-line-mid"></div>
          <div class="doc-line doc-line-short"></div>
        </div>
        <div class="doc-card card-blue"></div>
        <div class="doc-card card-green"></div>
        <div class="doc-card card-purple"></div>
        <div class="doc-panel"></div>
      </div>

      <div class="docs-copy">
        <h2 id="docs-tab-title">{{ t('workItems.docs.title') }}</h2>
        <p>{{ t('workItems.docs.description') }}</p>
        <div class="docs-actions" :aria-label="t('workItems.docs.actionsLabel')">
          <button
            class="docs-primary"
            type="button"
            disabled
            :title="t('workItems.docs.tryNeedsFlow')"
          >
            {{ t('workItems.docs.tryConfluence') }}
          </button>
          <button
            class="docs-link"
            type="button"
            disabled
            :title="t('workItems.docs.discoverNeedsFlow')"
          >
            {{ t('workItems.docs.discoverConfluence') }}
          </button>
        </div>
        <p class="docs-flow-note">{{ t('workItems.docs.connectNeedsFlow') }}</p>
      </div>
    </div>
  </section>
</template>

<script setup>
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()
</script>

<style scoped>
.docs-tab {
  flex: 1;
  min-height: calc(100vh - 150px);
  background: var(--color-bg);
  padding: 18px 18px 40px;
}

.docs-connect-banner {
  min-height: 120px;
  display: flex;
  align-items: center;
  gap: 14px;
  border-radius: 4px;
  background: #deebff;
  padding: 18px 22px;
  color: #172b4d;
}

.docs-illustration {
  position: relative;
  width: 160px;
  height: 82px;
  flex: 0 0 160px;
  overflow: hidden;
  background: #b3d4ff;
}

.doc-window {
  position: absolute;
  left: 15px;
  top: 18px;
  width: 74px;
  height: 44px;
  background: #ffffff;
  border: 1px solid #a6c5e7;
  box-shadow: 0 3px 0 rgba(9, 30, 66, 0.08);
  padding: 10px 11px;
}

.doc-bar,
.doc-line {
  height: 3px;
  border-radius: 2px;
  background: #c1c7d0;
}

.doc-bar {
  width: 30px;
  margin-bottom: 9px;
  background: #6b778c;
}

.doc-line {
  margin-top: 6px;
}

.doc-line-long {
  width: 44px;
  background: #36b37e;
}

.doc-line-mid {
  width: 35px;
  background: #ffab00;
}

.doc-line-short {
  width: 24px;
  background: #6554c0;
}

.doc-card {
  position: absolute;
  width: 18px;
  height: 18px;
  border-radius: 2px;
  box-shadow: 0 2px 4px rgba(9, 30, 66, 0.16);
}

.card-blue {
  right: 58px;
  top: 22px;
  background: #4c9aff;
  transform: rotate(-9deg);
}

.card-green {
  right: 35px;
  top: 27px;
  background: #abf5d1;
  transform: rotate(5deg);
}

.card-purple {
  right: 11px;
  top: 21px;
  background: #eae6ff;
  transform: rotate(-6deg);
}

.doc-panel {
  position: absolute;
  left: 8px;
  bottom: 8px;
  width: 110px;
  height: 13px;
  background: #ffffff;
  border: 1px solid #a6c5e7;
}

.docs-copy {
  min-width: 0;
}

.docs-copy h2 {
  margin: 0 0 8px;
  font-size: 17px;
  font-weight: 700;
  line-height: 1.2;
  letter-spacing: 0;
}

.docs-copy p {
  max-width: 900px;
  margin: 0;
  font-size: 13px;
  line-height: 1.45;
}

.docs-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 12px;
}

.docs-primary,
.docs-link {
  height: 28px;
  border-radius: 3px;
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0;
  cursor: not-allowed;
}

.docs-primary {
  border: 1px solid #0c66e4;
  background: #0c66e4;
  color: #ffffff;
  padding: 0 12px;
}

.docs-link {
  border: 0;
  background: transparent;
  color: #172b4d;
  padding: 0 2px;
}

.docs-primary:disabled,
.docs-link:disabled {
  opacity: 0.72;
}

.docs-flow-note {
  margin-top: 8px !important;
  color: #5e6c84;
}

@media (max-width: 720px) {
  .docs-connect-banner {
    align-items: flex-start;
    flex-direction: column;
  }

  .docs-actions {
    align-items: flex-start;
    flex-direction: column;
    gap: 8px;
  }
}

/* Compact density */
.docs-page,
.docs-tab {
  padding: 14px var(--sa-page-x, 24px) 28px !important;
}

.docs-connect-banner {
  border-radius: 8px !important;
  padding: 14px 16px !important;
}

.docs-copy h2 {
  font-size: 15px !important;
}

.docs-copy p {
  font-size: 12.5px !important;
}

.docs-primary,
.docs-link {
  height: 28px !important;
  border-radius: 7px !important;
  font-size: 12px !important;
}

@media (max-width: 720px) {
  .docs-page,
  .docs-tab {
    padding: 12px !important;
  }
}
</style>
