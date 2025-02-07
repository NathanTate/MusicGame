const submitBtn = document.getElementById('btn');
const input = document.getElementById('input')

submitBtn.addEventListener('click', getTeam.bind(null, input.value, startoffeset), false);

async function getTeam(userName, startOffset) {
  try {
    let satisfied = false;
    let offset = startOffset;
    while (!satisfied || aborted) {
      const response = await fetch(`https://www.hltv.org/fantasy/504/leagues/league/185103/json?offset=${offset}`, {
        headers: {
          'Access-Control-Allow-Origin': '*'
        }
      })
      const data = await response.json();
      const teams = data.phaseOverviews[0].leaderBoardData.teams;
      const filteredTeams = teams.filter(x => x.username.toLowerCase().includes(userName.toLowerCase()));
      if (filteredTeams.length > 0) {
        satisfied = true
        console.log(filteredTeams)
      }
      offset += 10;
    }
  } catch (err) {
    console.log(err);
  }
}
