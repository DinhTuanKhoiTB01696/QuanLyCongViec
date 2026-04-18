import axiosClient from './axiosClient';

export const adminUserApi = {
  getUsers(params) {
    return axiosClient.get('/admin/users', { params });
  },
  suspendUser(userId) {
    return axiosClient.put(`/admin/users/${userId}/suspend`);
  },
  createUser(data) {
    return axiosClient.post('/admin/users', data);
  },
  removeUser(userId) {
    return axiosClient.delete(`/admin/users/${userId}`);
  }
};
