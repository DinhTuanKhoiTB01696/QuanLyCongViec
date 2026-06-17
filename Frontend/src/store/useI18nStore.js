import { defineStore } from 'pinia'

const dictionary = {
  en: {
    'Teams': 'Teams',
    'Goals': 'Goals',
    'Projects': 'Projects',
    'People': 'People',
    'Tools': 'Tools',
    'Notifications': 'Notifications',
    'Status': 'Status',
    'Starred': 'Starred',
    'Audit Log': 'Audit Log',
    'Home': 'Home',
    'Settings': 'Settings',
    'Search': 'Search SprintA',
    'Language': 'Language',
    'My profile': 'My profile',
    'Account settings': 'Account settings',
    'Sign out': 'Sign out',
    'Site Management': 'Site Management',
    'Create': 'Create',
    'Cancel': 'Cancel',
    'Welcome back': 'Welcome back',
    'Pick up where you left off in SprintA': 'Pick up where you left off in SprintA',
    'Create a new site': 'Create a new site',
    'Looking for a different site?': 'Looking for a different site?',
    'Go to SprintA': 'Go to SprintA',
    'Site Name': 'Site Name',
    'Enter site name': 'Enter site name',
    'Creating...': 'Creating...',
    'Choose your workspace to continue': 'Choose your workspace to continue',
    'Continue with recent site': 'Continue with recent site',
    'Continue with new site': 'Continue with new site',
    'Sites': 'Sites',
    'Search sites...': 'Search sites...',
    'Add Site': 'Add Site',
    'List view': 'List view',
    'Grid view': 'Grid view',
    'Created date': 'Created date',
    'Visibility': 'Visibility',
    'All sites': 'All sites',
    'Public': 'Public',
    'Private': 'Private',
    'Clear filters': 'Clear filters',
    'No sites found': 'No sites found',
    'It looks like there are no sites here. Let\'s create your first one!': 'It looks like there are no sites here. Let\'s create your first one!',
    'Create your first site': 'Create your first site',
    'Archive site': 'Archive site',
    'Search work items...': 'Search work items...',
    'Filters': 'Filters'
  },
  vi: {
    'Teams': 'Nhóm',
    'Goals': 'Mục tiêu',
    'Projects': 'Dự án',
    'People': 'Thành viên',
    'Tools': 'Công cụ',
    'Notifications': 'Thông báo',
    'Status': 'Trạng thái',
    'Starred': 'Đánh dấu sao',
    'Audit Log': 'Nhật ký hoạt động',
    'Home': 'Trang chủ',
    'Settings': 'Cài đặt',
    'Search': 'Tìm kiếm SprintA',
    'Language': 'Ngôn ngữ',
    'My profile': 'Hồ sơ của tôi',
    'Account settings': 'Cài đặt tài khoản',
    'Sign out': 'Đăng xuất',
    'Site Management': 'Quản lý Site',
    'Create': 'Tạo mới',
    'Cancel': 'Hủy',
    'Welcome back': 'Chào mừng quay lại',
    'Pick up where you left off in SprintA': 'Tiếp tục công việc của bạn tại SprintA',
    'Create a new site': 'Tạo một site mới',
    'Looking for a different site?': 'Đang tìm một site khác?',
    'Go to SprintA': 'Đến SprintA',
    'Site Name': 'Tên Site',
    'Enter site name': 'Nhập tên site',
    'Creating...': 'Đang tạo...',
    'Choose your workspace to continue': 'Chọn workspace của bạn để tiếp tục',
    'Continue with recent site': 'Tiếp tục với site cũ gần nhất',
    'Continue với new site': 'Tiếp tục với site mới',
    'Sites': 'Các Site',
    'Search sites...': 'Tìm kiếm site...',
    'Add Site': 'Thêm Site',
    'List view': 'Dạng danh sách',
    'Grid view': 'Dạng lưới',
    'Created date': 'Ngày tạo',
    'Visibility': 'Quyền truy cập',
    'All sites': 'Tất cả các site',
    'Public': 'Công khai',
    'Private': 'Riêng tư',
    'Clear filters': 'Xóa bộ lọc',
    'No sites found': 'Không tìm thấy site nào',
    'It looks like there are no sites here. Let\'s create your first one!': 'Có vẻ như chưa có site nào ở đây. Hãy tạo site đầu tiên của bạn!',
    'Create your first site': 'Tạo site đầu tiên của bạn',
    'Archive site': 'Lưu trữ site',
    'Search work items...': 'Tìm kiếm công việc...',
    'Filters': 'Bộ lọc'
  }
}

export const useI18nStore = defineStore('i18n', {
  state: () => ({
    locale: localStorage.getItem('sprinta_locale') || 'en'
  }),
  getters: {
    t: (state) => (key) => {
      const texts = dictionary[state.locale]
      if (texts && texts[key]) return texts[key]
      return key
    }
  },
  actions: {
    setLocale(newLocale) {
      if (dictionary[newLocale]) {
        this.locale = newLocale
        localStorage.setItem('sprinta_locale', newLocale)
      }
    }
  }
})
