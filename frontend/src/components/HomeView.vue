<template>
  <div class="home-view">
    <h1 class="title"> DCR Blockchain </h1>
    <div class="top-controls">
      <input class="search-input" type="text" placeholder="Graph ID" v-model="searchId" />
      <div class="button-container">
        <button class="submit-button" :disabled="searchId == ''" @click="searchGraph"> Find graph </button>
        or
        <button class="submit-button" @click="newGraph"> New graph </button>
      </div>
      <h4> {{ executeMode ? 'You are in execution mode: Press on an activity\'s Executed field to execute it' : 'You are in creation mode: Add activities and relations to your graph, and finish by pressing Create graph' }}</h4>
      <h4 v-if="executeMode">The current graph {{ isAccepting ? 'accepting' : 'not accepting' }}</h4>
    </div>
    
    <div class="tables-container">
      <div class="table-container">
        <h2>Activities</h2>
        <TableComponent :headers="activityHeaders" :data="activities" :executeMode="executeMode" @executeActivity="executeActivity"/>
        <button class="submit-button" :disabled="executeMode" @click="addActivity"> Add activity </button>
      </div>

      <div class="table-container">
        <h2>Relations</h2>
        <TableComponent :headers="relationHeaders" :data="relations" :activityTitles="activityTitles" :relationTypes="relationTypes" :executeMode="executeMode"/>
        <button class="submit-button" :disabled="executeMode" @click="addRelation"> Add relation </button>
      </div>
    </div>
    <button class="submit-button" @click="createGraph" :hidden="executeMode"> Create graph </button>
  </div>
</template>

<script>
import "./HomeView.scss";
import axios from "../js/axios.config"
import TableComponent from "./TableComponent.vue";

export default {
  name: 'HomeView',
  components: {
    TableComponent
  },

  data() {
    return {
      searchId: "",
      executeMode: false,
      isAccepting: false,
      activityHeaders: [
        {title: "Title", type: "text"},
        {title: "Pending", type: "checkbox"},
        {title: "Included", type: "checkbox"},
        {title: "Executed", type: "checkbox"},
        {title: "Enabled", type: "checkbox"}],
      activities: [
        { title: "Select papers", pending: true, included: true, executed: false, enabled: true },
        { title: "Write introduction", pending: true, included: true, executed: false, enabled: false },
        { title: "Write abstract", pending: true, included: true, executed: false, enabled: false},
        { title: "Write conclusion", pending: true, included: true, executed: false, enabled: false },
      ],
      relationHeaders: [
        {title: "Source", type: "select activity"},
        {title: "Type", type: "select relation"},
        {title: "Target", type: "select activity"}
      ],
      relations: [
        { source: "Select papers", type: 2, target: "Select papers" },
        { source: "Select papers", type: 0, target: "Write introduction" },
        { source: "Select papers", type: 0, target: "Write abstract" },
        { source: "Select papers", type: 0, target: "Write conclusion" },
        { source: "Write introduction", type: 1, target: "Write abstract" },
        { source: "Write conclusion", type: 1, target: "Write abstract" },
      ],
      relationTypes: [
        {id: 0, text: '-->* (condition)'},
        {id: 1, text: '*--> (response)'},
        {id: 2, text: '-->% (excludes)'},
        {id: 3, text: '-->+ (includes)'},
      ],
    }
  },

  computed: {
    activityTitles() {
      return this.activities.map(item => item.title);
    }
  },

  methods: {
    async searchGraph() {
      await axios.get(`DCR/${this.searchId}`).then(res => {
        if (res.status == 200) {
          this.activities = res.data['activities'];
          this.relations = res.data['relations'];
          this.isAccepting = res.data['accepting'];
          this.executeMode = true;
        }
      }).catch(err => {
          console.log(err);
      });
    },

    newGraph() {
      this.activities = [{ title: "", pending: false, included: true, executed: false, enabled: true }];
      this.relations = [{ source: "", type: null, target: "" }];
      this.executeMode = false;
    },

    addActivity() {
      this.activities.push({ title: "", pending: false, included: true, executed: false, enabled: true });
    },

    addRelation() {
      this.relations.push({ source: "", type: null, target: "" });
    },

    async createGraph() {
      const payload = {"Actor": "Foo", "Activities": this.activities, "Relations": this.relations};
      await axios.post(`DCR/create`, payload).then(res => {
        if (res.status == 200) {
          this.searchId = res.data['id'];
          this.isAccepting = res.data['accepting'];
          this.executeMode = true;
        }
      }).catch(err => {
          console.log(err);
      });
    },

    async executeActivity(title) {
      const payload = {'actor': "1", 'executingActivity': title};
      await axios.put(`DCR/update/${this.searchId}`, payload).then(res => {
        if (res.status == 200) {
          this.activities = res.data['activities'];
          this.relations = res.data['relations'];
          this.isAccepting = res.data['accepting'];
        }
      }).catch(err => {
          console.log(err);
      });
    }
  }
}
</script>

<style scoped lang="scss" />