<template>
  <div class="home-view">
    <h1 class="title"> DCR Blockchain </h1>
    <div class="top-controls">
      <input class="search-input" type="text" placeholder="Graph ID" v-model="searchId" />
      <div class="button-container">
        <button class="submit-button" :disabled="searchId == ''" @click="searchGraph(searchId)"> Find graph </button>
        or
        <button class="submit-button" @click="newGraph"> New graph </button>
      </div>
    </div>

    <div class="seperator"></div>

    <!-- Overview -->
    <h2>Overview</h2>
    <span> See discovered graphs and pending transactions </span>
    <div class="tables-container">
      <div class="table-container">
        <h3>Discovered Graphs</h3>
        <LinkTableComponent :headers="discoveredGraphsHeaders" :data="discoveredGraphs" @showGraph="showGraph"/>
      </div>

      <div class="table-container">
        <h3>Pending Transactions</h3>
        <TableComponent :headers="pendingTransactionsHeaders" :data="pendingTransactions" :selectedGraph="currentGraphId" :disabled="true" />
      </div>
    </div>

    <div class="seperator"></div>
    
    <!-- Current Graph -->
    <h2>Current Graph</h2>
    <span> <b> You are in {{ executeMode ? 'execution' : 'creation' }} mode </b> </span>
    <span> {{ executeMode ? 'Execute an activity by pressing its Execute button' : 'Add activites and relations to the graph, and press "Create graph" to start execution' }} </span>
    <h4 v-if="executeMode">The current graph is {{ isAccepting ? 'accepting' : 'not accepting' }}</h4>
    
    <div class="tables-container">
      <div class="table-container">
        <h3>Activities</h3>
        <TableComponent :headers="activityHeaders" :data="activities" :executeMode="executeMode" @executeActivity="executeActivity" :disabled="graphHasPendingTransactions" /> <!-- Add disabled="hasPendingTransactions"? -->
        <button class="submit-button" :disabled="executeMode" @click="addActivity"> Add activity </button>
      </div>

      <div class="table-container">
        <h3>Relations</h3>
        <TableComponent :headers="relationHeaders" :data="relations" :activityTitles="activityTitles" :relationTypes="relationTypes" :executeMode="executeMode" :disabled="graphHasPendingTransactions" /> <!-- Add disabled="hasPendingTransactions"? -->
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
import LinkTableComponent from "./LinkTableComponent.vue";
import * as signalR from '@aspnet/signalr';

export default {
  name: 'HomeView',
  components: {
    TableComponent,
    LinkTableComponent
  },

  data() {
    return {
      searchId: "",
      currentGraphId: "",
      executeMode: false,
      isAccepting: false,
      activityHeaders: [
        {title: "Title", mapping: "title", type: "text"},
        {title: "Pending", mapping: "pending", type: "checkbox"},
        {title: "Included", mapping: "included", type: "checkbox"},
        {title: "Executed", mapping: "executed", type: "button"}
      ],
      activities: [
        { title: "Select papers", pending: true, included: true, executed: false, enabled: true },
        { title: "Write introduction", pending: true, included: true, executed: false, enabled: false },
        { title: "Write abstract", pending: true, included: true, executed: false, enabled: false},
        { title: "Write conclusion", pending: true, included: true, executed: false, enabled: false },
      ],
      relationHeaders: [
        {title: "Source", mapping: "source", type: "select activity"},
        {title: "Type", mapping: "type", type: "select relation"},
        {title: "Target", mapping: "target", type: "select activity"}
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
      discoveredGraphsHeaders: [
        {title: "Graph ID", mapping: "id"}
        // {title: "Accepting", mapping: "accepting"},
      ],
      discoveredGraphs: [],
      pendingTransactionsHeaders: [
        {title: "Graph ID", mapping: "graphId", type: "text"},
        {title: "Action", mapping: "action", type: "text"},
        {title: "Target Activity", mapping: "activity", type: "text"}
      ],
      pendingTransactions: []
    }
  },

  computed: {
    activityTitles() {
      return this.activities.map(item => item.title);
    },

    hasPendingGraphCreation() {
      return this.pendingTransactions.some(transaction => (transaction['graphId'] == this.currentGraphId) && transaction['action'] == "Create graph");
    },

    graphHasPendingTransactions() {
      return this.pendingTransactions.some(transaction => transaction['graphId'] == this.currentGraphId);
    }
  },

  methods: {
    async searchGraph(id) {
      await axios.get(`DCR/graph/${id}`).then(res => {
        if (res.status == 200) {
          this.activities = res.data['activities'];
          this.relations = res.data['relations'];
          this.isAccepting = res.data['accepting'];
          this.currentGraphId = this.searchId;
          this.executeMode = true;

          if (!this.discoveredGraphs.some(graph => graph['id'] == id)) {
            this.getDiscoveredGraphs();
          }
        }
      }).catch(err => {
          console.log(err);
      });
    },

    showGraph(id) {
      this.searchId = id;
      this.searchGraph(id);
    },

    newGraph() {
      this.activities = [{ title: "", pending: false, included: true, executed: false, enabled: true }];
      this.relations = [{ source: "", type: null, target: "" }];
      this.searchId = "";
      this.currentGraphId = "";
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
          this.currentGraphId = res.data['id'];
          this.isAccepting = res.data['accepting'];
          this.executeMode = true;
          this.getPendingTransactions();
        }
      }).catch(err => {
          console.log(err);
      });
    },

    async executeActivity(activityTitle) {
      const payload = {'actor': "1", 'executingActivity': activityTitle};
      await axios.put(`DCR/update/${this.currentGraphId}`, payload).then(res => {
        if (res.status == 200) {
          this.getPendingTransactions();
        }
      }).catch(err => {
          console.log(err);
      });
    },

    async getDiscoveredGraphs() {
      await axios.get(`DCR/discovered`).then(res => {
        if (res.status == 200) {
          this.discoveredGraphs = this.formatDiscoveredGraphs(res.data);
        }
      }).catch(err => {
          console.log(err);
      });
    },

    async getPendingTransactions() {
      await axios.get(`DCR/pending`).then(res => {
        if (res.status == 200) {
          const updatedTransactions = this.formatPendingTransactions(res.data);
          // If there is a pending transaction for current graph, and updated transactions resolves this transaction, fetch updated graph automatically
          if (this.pendingTransactions.some(pt => (pt['graphId'] == this.currentGraphId) && !updatedTransactions.some(ut => ut['graphId'] == pt['graphId']))) {
            this.searchGraph(this.currentGraphId);
          }
          this.pendingTransactions = updatedTransactions;
        }
      }).catch(err => {
          console.log(err);
      });
    },

    formatDiscoveredGraphs(graphs) {
      return graphs.map((graph) => ({
        'id': graph['id'],
        'accepting': graph['accepting']
      }));
    },

    formatPendingTransactions(transactions) {
      return transactions.map((transaction) => ({
        'graphId': transaction['graph']['id'],
        'action': transaction['entityTitle'] ? 'Execute activity' : 'Create graph',
        'activity': transaction['entityTitle']
      }));
    }
  },

  created() {
    this.getDiscoveredGraphs();
    this.getPendingTransactions();

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:${process.env.VUE_APP_BACKEND}/pending-transactions-hub`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: false
      })
      .build();

    this.connection.on('update', () => {
      this.getPendingTransactions();
    });

    this.connection.start().catch(err => console.error(err));
  },
}
</script>

<style scoped lang="scss" />