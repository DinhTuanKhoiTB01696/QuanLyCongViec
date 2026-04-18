import { defineStore } from 'pinia';
import { ref } from 'vue';
import { adminUserApi } from '@/api/adminUserApi';

export const useAdminUserStore = defineStore('adminUsers', () => {
  const users = ref([]);
  const loading = ref(false);

  const fetchUsers = async (search = '') => {
    loading.value = true;
    try {
      const res = await adminUserApi.getUsers({ search });
      if (res.data && res.data.data) {
        users.value = res.data.data;
      }
    } catch (error) {
      console.error('Failed to load users:', error);
    } finally {
      loading.value = false;
    }
  };

  const suspendUser = async (userId) => {
    try {
      await adminUserApi.suspendUser(userId);
      const userIndex = users.value.findIndex(user => user.id === userId);
      if (userIndex !== -1) {
        users.value[userIndex].isActive = false;
        users.value[userIndex].status = 'Suspended';
      }
      return true;
    } catch (error) {
      console.error('Failed to suspend user:', error);
      throw error;
    }
  };

  const createUser = async (data) => {
    loading.value = true;
    try {
      await adminUserApi.createUser(data);
      await fetchUsers();
      return true;
    } catch (error) {
      console.error('Failed to create user:', error);
      throw error;
    } finally {
      loading.value = false;
    }
  };

  const inviteUsers = async (payloads) => {
    loading.value = true;
    try {
      await Promise.all(payloads.map(payload => adminUserApi.createUser(payload)));
      await fetchUsers();
      return true;
    } catch (error) {
      console.error('Failed to invite users:', error);
      throw error;
    } finally {
      loading.value = false;
    }
  };

  const removeUser = async (userId) => {
    loading.value = true;
    try {
      await adminUserApi.removeUser(userId);
      await fetchUsers();
      return true;
    } catch (error) {
      console.error('Failed to remove user:', error);
      throw error;
    } finally {
      loading.value = false;
    }
  };

  return { users, loading, fetchUsers, suspendUser, createUser, inviteUsers, removeUser };
});
