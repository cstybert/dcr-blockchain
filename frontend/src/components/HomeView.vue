<template>
  <div class="home-view">
    <h1 class="title"> DCR BlockChain </h1>
    <div class="top-controls">
      <input type="text" placeholder="Graph ID" v-model="searchId" />
      <div class="button-container">
        <button class="submit-button" :disabled="searchId == ''" @click="searchGraph"> Find graph </button>
        or
        <button class="submit-button" @click="newGraph"> New graph </button>
      </div>
    </div>
    
    <div class="tables-container">
      <div class="table-container">
        <h2>Activities</h2>
        <TableComponent :headers="activityHeaders" :data="activities"/>
        <button class="submit-button" @click="addActivity"> Add activity </button>
      </div>

      <div class="table-container">
        <h2>Relations</h2>
        <TableComponent :headers="relationHeaders" :data="relations" :activityTitles="activityTitles" :relationTypes="relationTypes"/>
        <button class="submit-button" @click="addRelation"> Add relation </button>
      </div>
    </div>
    <button class="submit-button" @click="createGraph" :hidden="!isNewGraph"> Create graph </button>
  </div>
</template>

<script>
import "./HomeView.scss";
import blockchainApi from "../services/blockchain.js";
import TableComponent from "./TableComponent.vue";

export default {
  name: 'HomeView',
  components: {
    TableComponent
  },

  data() {
    return {
      searchId: "",
      isNewGraph: false,
      activityHeaders: [
        {title: "Title", type: "text"},
        {title: "Pending", type: "checkbox"},
        {title: "Included", type: "checkbox"},
        {title: "Executed", type: "checkbox"},
        {title: "Enabled", type: "checkbox"}],
      activities: [
        { title: "A", pending: true, included: true, executed: false, enabled: true },
        { title: "B", pending: true, included: false, executed: false, enabled: false },
        { title: "C", pending: false, included: true, executed: false, enabled: true},
        { title: "D", pending: false, included: true, executed: false, enabled: true },
      ],
      relationHeaders: [
        {title: "Source", type: "select activity"},
        {title: "Type", type: "select relation"},
        {title: "Target", type: "select activity"}
      ],
      relationTypes: [
        {id: 0, text: '-->* (condition)'},
        {id: 1, text: '*--> (response)'},
        {id: 2, text: '-->% (excludes)'},
        {id: 3, text: '-->+ (includes)'},
      ],
      relations: [
        { source: "A", type: 0, target: "B" },
        { source: "B", type: 1, target: "C" },
        { source: "C", type: 2, target: "D" },
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
      const graph = await blockchainApi.getGraph(this.searchId);
      this.activities = graph['activities'];
      this.relations = graph['relations'];
      this.isNewGraph = false;
    },

    newGraph() {
      this.activities = [{ title: "", pending: false, included: true, executed: false, enabled: true }];
      this.relations = [{ source: "", type: null, target: "" }];
      this.isNewGraph = true;
    },

    addActivity() {
      this.activities.push({ title: "", pending: false, included: true, executed: false, enabled: true });
    },

    addRelation() {
      this.relations.push({ source: "", type: null, target: "" });
    },

    createGraph() {
      // TODO: Wrap activities and relations in (graph) object and POST to DCR API
    }
  }
}
</script>

<style scoped lang="scss" />
