import axios from 'axios';

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
        const token = localStorage.getItem('accessToken');
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
                const accessToken = localStorage.getItem('accessToken');
                const authHeaders = accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {};
                const { data } = await axios.post(`${baseURL}/auth/refresh-token`, {}, {
                    headers: authHeaders,
                    withCredentials: true
                });

                const newAccessToken = data.data.accessToken;
                localStorage.setItem('accessToken', newAccessToken);

                axiosClient.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`;
                originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;

                processQueue(null, newAccessToken);
                return axiosClient(originalRequest);
            } catch (err) {
                processQueue(err, null);
                // Refresh token failed (expired or invalid), force logout
                localStorage.removeItem('accessToken');
                localStorage.removeItem('user');
                window.location.href = '/login'; // Redirect to login
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
