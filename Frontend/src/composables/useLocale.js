import { ref, computed } from 'vue';

// Module-level shared ref — all components share the same locale state
const currentLocale = ref(localStorage.getItem('admin_locale') || 'vi');

export const useLocale = () => {
    const toggleLocale = () => {
        currentLocale.value = currentLocale.value === 'vi' ? 'en' : 'vi';
        localStorage.setItem('admin_locale', currentLocale.value);
    };

    /**
     * Translate helper: returns English or Vietnamese string based on current locale.
     * Since this reads currentLocale.value, Vue will track the dependency
     * and re-render when locale changes.
     */
    const t = (en, vi) => {
        return currentLocale.value === 'vi' ? vi : en;
    };

    const formatDateLocal = (isoString) => {
        if (!isoString) return '';
        const d = new Date(isoString);
        if (isNaN(d.getTime())) return '';
        return d.toLocaleString(currentLocale.value === 'vi' ? 'vi-VN' : 'en-US');
    };

    return { locale: currentLocale, toggleLocale, t, formatDateLocal };
};
