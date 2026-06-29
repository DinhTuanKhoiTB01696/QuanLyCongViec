import axiosClient from './axiosClient';

export const uploadApi = {
  uploadImage(file) {
    const formData = new FormData();
    formData.append('file', file);
    return axiosClient.post('/Uploads/image', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  uploadFile(file) {
    const formData = new FormData();
    formData.append('file', file);
    return axiosClient.post('/Uploads/file', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
};
