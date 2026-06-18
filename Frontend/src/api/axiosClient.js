import axios from 'axios';
import { clearAuthSession, getStoredAccessToken } from '@/utils/authSession'

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5136/api';

const axiosClient = axios.create({
    baseURL: baseURL,
    headers: {
        'Content-Type': 'application/json'
    },
    withCredentials: true // Rất quan trọng để đính kèm HttpOnly cookies (RefreshToken)
});

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
    failedQueue.forEach(prom => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });

    failedQueue = [];
};

axiosClient.interceptors.request.use(
    (config) => {
        const token = getStoredAccessToken();
        if (token) {
            config.headers['Authorization'] = `Bearer ${token}`;
        }
        const locale = localStorage.getItem('admin_locale') || 'vi';
        config.headers['Accept-Language'] = locale;
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

axiosClient.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error) => {
        const originalRequest = error.config;

        const isAuthRequest = originalRequest.url.includes('/auth/login') || 
                              originalRequest.url.includes('/auth/register') ||
                              originalRequest.url.includes('/auth/send-otp') ||
                              originalRequest.url.includes('/auth/verify-otp') ||
                              originalRequest.url.includes('/auth/reset-password') ||
                              originalRequest.url.includes('/auth/google-login') ||
                              originalRequest.url.includes('/auth/github-login') ||
                              originalRequest.url.includes('/auth/invite-info') ||
                              originalRequest.url.includes('/auth/accept-invite-token');

        if (error.response?.status === 401 && !originalRequest._retry && !isAuthRequest) {
            if (isRefreshing) {
                return new Promise(function (resolve, reject) {
                    failedQueue.push({ resolve, reject });
                }).then(token => {
                    originalRequest.headers['Authorization'] = 'Bearer ' + token;
                    return axiosClient(originalRequest);
                }).catch(err => {
                    return Promise.reject(err);
                });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
                // Call refresh-token API. Cookie is automatically sent due to withCredentials
                // Backend requires the expired access token in the header to identify the user
                const accessToken = getStoredAccessToken();
                const authHeaders = accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {};
                const { data } = await axios.post(`${baseURL}/auth/refresh-token`, {}, {
                    headers: authHeaders,
                    withCredentials: true
                });

                const newAccessToken = data.data.accessToken;
                sessionStorage.setItem('accessToken', newAccessToken);
                localStorage.removeItem('accessToken');

                axiosClient.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`;
                originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;

                processQueue(null, newAccessToken);
                return axiosClient(originalRequest);
            } catch (err) {
                processQueue(err, null);
                const refreshStatus = err?.response?.status
                const shouldForceLogout = refreshStatus === 401 || refreshStatus === 403

                if (shouldForceLogout) {
                    clearAuthSession();
                    const currentPath = `${window.location.pathname}${window.location.search}${window.location.hash}`;
                    const redirect = currentPath && !currentPath.startsWith('/login')
                        ? `?redirect=${encodeURIComponent(currentPath)}`
                        : '';
                    window.location.href = `/login${redirect}`;
                }

                return Promise.reject(err);
            } finally {
                isRefreshing = false;
            }
        }

        if (error.response && error.response.status === 409) {
            console.warn('Conflict detected:', error.response.data);
            // Optionally, we could emit a global event or show a notification
            // but for now, we'll let the component handle the catch block
        }

        return Promise.reject(error);
    }
);

export default axiosClient;
