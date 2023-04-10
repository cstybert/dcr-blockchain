import Axios from 'axios';

const backendPort = process.env.VUE_APP_BACKEND || '4300';
const axios = Axios.create({
  baseURL: `http://localhost:${backendPort}`,
  timeout: 40000,
  crossDomain: true
});

export default axios;